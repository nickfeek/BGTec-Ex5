using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnprFileService.Models;
using AnprFileService.Data;
using System.ComponentModel.DataAnnotations;

namespace AnprFileService
{
    // Background service for monitoring and processing files
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _watchedDirectory;
        private FileSystemWatcher _watcher = new FileSystemWatcher();

        // Constructor for Worker class
        public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _watchedDirectory = configuration["WatchedDirectory"] ?? throw new ArgumentNullException(nameof(configuration));

            // Ensure the database and table are created
            EnsureDatabaseCreated();

            // Ensures the default watcher directory created
            EnsureWatcherDirecoryCreated();


            // Initialize the FileSystemWatcher
            InitializeFileSystemWatcher();
        }

        // Ensures the database and table are created
        private void EnsureDatabaseCreated()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dataRepository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
                try
                {
                    dataRepository.EnsureDatabaseCreatedAsync().Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while ensuring the database is created.");
                    throw;
                }
            }
        }
        
        // Ensures the default watcher directory created
        private void EnsureWatcherDirecoryCreated()
        {

            // Directory path
            string path = _watchedDirectory;

            // Check if the directory exists
            if (!Directory.Exists(path))
            {
                // Create the directory
                Directory.CreateDirectory(path);
                _logger.LogInformation($"Directory created at: {path}");
            }
            else
            {
                _logger.LogInformation($"Directory already exists at: {path}");
            }
        }



        // Initializes the FileSystemWatcher
        private void InitializeFileSystemWatcher()
        {
            _watcher = new FileSystemWatcher(_watchedDirectory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true // Watch subdirectories
            };

            // Subscribe to the Created event
            _watcher.Created += OnFileOrDirectoryCreated;
        }

        // Background execution logic
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started.");

            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Worker stopping due to cancellation.");

                _watcher.Created -= OnFileOrDirectoryCreated;
                _watcher.Dispose();

                _logger.LogInformation("FileSystemWatcher disposed.");
            });

            return Task.CompletedTask;
        }

        // Handles the file or directory created event
        private void OnFileOrDirectoryCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                _logger.LogInformation($"New item detected: {e.FullPath}");

                if (File.Exists(e.FullPath))
                {
                    ProcessFile(e.FullPath);
                }
                else if (Directory.Exists(e.FullPath))
                {
                    ProcessFilesInDirectory(e.FullPath);
                }
                else
                {
                    _logger.LogWarning($"Detected item is neither a file nor a directory: {e.FullPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while processing item '{e.FullPath}'.");
            }
        }

        // Method to process files in the directory and its subdirectories recursively
        private void ProcessFilesInDirectory(string directoryPath)
        {
            try
            {
                // Process files in the current directory
                ProcessFilesInCurrentDirectory(directoryPath);

                // Get all subdirectories in the current directory
                string[] subdirectories = Directory.GetDirectories(directoryPath);

                // Recursively process files in each subdirectory
                foreach (string subdirectory in subdirectories)
                {
                    ProcessFilesInDirectory(subdirectory);
                }
            }
            catch (Exception ex)
            {
                // Log error if an exception occurs during processing
                _logger.LogError(ex, $"An error occurred while processing files in directory '{directoryPath}'.");
            }
        }

        // Method to process files in the current directory
        private void ProcessFilesInCurrentDirectory(string directoryPath)
        {
            try
            {
                // Get all .lpr files in the current directory
                string[] files = Directory.GetFiles(directoryPath, "*.lpr", SearchOption.TopDirectoryOnly);

                // Process each file
                foreach (string file in files)
                {
                    ProcessFile(file);
                }
            }
            catch (Exception ex)
            {
                // Log error if an exception occurs during processing
                _logger.LogError(ex, $"An error occurred while processing files in directory '{directoryPath}'.");
            }
        }

        // Method to process the file
        private void ProcessFile(string filePath)
        {
            int maxRetries = 5;
            int initialDelay = 500; // Initial delay in milliseconds

            // Read all lines from the file with retry logic
            List<string> lines = ReadAllLinesWithRetry(filePath, maxRetries, initialDelay);

            // Process each line in the file
            foreach (var line in lines)
            {
                _logger.LogInformation($"Processing line: {line}");

                // Skip empty or whitespace lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    _logger.LogWarning($"Skipping empty or whitespace line in file: {filePath}");
                    continue;
                }

                // Split the line into fields using '\\' or '/' as delimiters
                if (!SplitLineIntoFields(line, filePath, out string[] fields))
                {
                    _logger.LogWarning($"Skipping empty record in file: {filePath}");
                    continue;
                }

                // Get the root directory
                string rootDirectory = _watchedDirectory;
                // Get the relative path of the file
                string relativePath = filePath.Substring(rootDirectory.Length);

                // Check if the file record already exists in the database
                bool recordExists = FileRecordExistsInDatabase(filePath).Result;
                if (recordExists)
                {
                    _logger.LogWarning($"Duplicate file record found in the database for file path: {relativePath}");
                    continue;
                }

                FileRecord fileRecord = CreateFileRecord(fields, relativePath);

                // Validate the file record
                if (!ValidateFileRecord(fileRecord))
                {
                    _logger.LogWarning($"Data invalid!");
                    continue;
                }

                // Save the file record to the database
                SaveFileRecordToDatabase(fileRecord).Wait();
            }
        }

        // Method to split the line into fields
        private bool SplitLineIntoFields(string line, string filePath, out string[] fields)
        {
            // Split the line by '\' or '/'
            fields = line.Split(new[] { '\\', '/' });

            // Check if the correct number of columns are present
            if (fields.Length != 7)
            {
                _logger.LogWarning($"Invalid file format: {filePath} incorrect number of columns.");
                return false;
            }
            return true;
        }

        // Method to check if a record with the same path already exists in the database
        private async Task<bool> FileRecordExistsInDatabase(string filePath)
        {
            // Get the root directory
            string rootDirectory = _watchedDirectory;
            // Get the relative path of the file
            string relativePath = filePath.Substring(rootDirectory.Length);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dataRepository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
                // Check if the file record exists in the database
                try
                {
                    return await dataRepository.FileRecordExistsAsync(relativePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while checking if file record exists in the database for file '{relativePath}'.");
                    throw;
                }
            }
        }

        // Method to validate the file record
        private bool ValidateFileRecord(FileRecord fileRecord)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(fileRecord, null, null);
            bool isValid = Validator.TryValidateObject(fileRecord, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    _logger.LogWarning($"Validation error: {validationResult.ErrorMessage}");
                }
            }

            return isValid;
        }

        // Method to create a new file record object from the fields
        private FileRecord CreateFileRecord(string[] fields, string filePath)
        {
            if (!int.TryParse(fields[4], out int dateInt))
            {
                _logger.LogWarning("Data error: Cannot convert Date to Int.");
            }

            if (!int.TryParse(fields[5], out int timeInt))
            {
                _logger.LogWarning("Data error: Cannot convert Time to Int.");
            }


            FileRecord fileRecord = new FileRecord{
                // Populate the FileRecord object
                CountryOfVehicle = fields[0],
                RegNumber = fields[1].TrimStart('r'),
                ConfidenceLevel = fields[2].TrimStart('r'),
                CameraName = fields[3].TrimStart('r'),
                Date = dateInt, 
                Time = timeInt, // null no problem, validation is downstream
                ImageFilename = fields[6],
                Path = filePath,
                CreatedAt = DateTime.UtcNow,
            };

            return fileRecord;
        }

        // Method to save the file record to the database
        private async Task SaveFileRecordToDatabase(FileRecord fileRecord)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dataRepository = scope.ServiceProvider.GetRequiredService<IDataRepository>();
                // Save the file record to the database
                try
                {
                    await dataRepository.SaveFileRecordAsync(fileRecord);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while saving file record to the database for file '{fileRecord.Path}'.");
                    throw;
                }
            }
        }

        // Method to read all lines from the file with sharing
        static List<string> ReadAllLinesWithSharing(string filePath)
        {
            var lines = new List<string>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        // Method to read all lines from a file with retry
        static List<string> ReadAllLinesWithRetry(string filePath, int maxRetries, int initialDelay)
        {
            int attempt = 0; // Initialize the attempt counter
            Random random = new Random(); // Create a random number generator

            while (true) // Continue indefinitely until successful or max retries exceeded
            {
                try
                {
                    // Try reading all lines from the file with file sharing
                    return ReadAllLinesWithSharing(filePath);
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    // Handle IOException indicating file access error (retryable)
                    attempt++; // Increment the attempt counter
                    int delay = initialDelay * (int)Math.Pow(2, attempt) + random.Next(0, 100);
                    Console.WriteLine($"Attempt {attempt} failed. Retrying in {delay}ms..."); // Log retry attempt
                    Thread.Sleep(delay); // Pause execution before retrying
                }
            }
        }

        // Clean up resources on service stop
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping the worker service...");

            // Dispose the FileSystemWatcher
            _watcher?.Dispose();

            await base.StopAsync(cancellationToken);

            _logger.LogInformation("Worker service stopped.");
        }
    }
}
