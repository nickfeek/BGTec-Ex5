using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnprFileService.Models;

namespace AnprFileService.Data
{
    // Implementation of the IDataRepository interface
    public class DataRepository : IDataRepository
    {
        // Private fields to store the database context and logger
        private readonly AppDbContext _context;
        private readonly ILogger<DataRepository> _logger;

        // Constructor to initialize the database context and logger
        public DataRepository(AppDbContext context, ILogger<DataRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Method to ensure the database and table are created
        public async Task EnsureDatabaseCreatedAsync()
        {
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database and table ensured.");
        }

        // Method to check if a file record with the specified path exists in the database
        public async Task<bool> FileRecordExistsAsync(string relativePath)
        {
            return await _context.Files.AnyAsync(f => f.Path == relativePath);
        }

        // Method to save a file record to the database
        public async Task SaveFileRecordAsync(FileRecord fileRecord)
        {
            try
            {
                // Add the file record to the context
                _context.Files.Add(fileRecord);
                // Save changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log an error if an exception occurs during the save operation
                _logger.LogError(ex, $"Error occurred while saving changes to the database: {ex.Message}");
            }
        }
    }
}
