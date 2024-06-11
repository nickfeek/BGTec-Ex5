using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AnprFileService.Data;

namespace AnprFileService
{
    public class Program
    {
        // Entry point of the application
        public static async Task Main(string[] args)
        {
            // Configure the host
            var builder = new HostBuilder()
                // Configure application settings
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                // Configure services
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure EF Core to use SQLite
                    var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

                    // Register the DataRepository as an implementation of IDataRepository
                    services.AddScoped<IDataRepository, DataRepository>();

                    // Add the Worker service as a hosted service
                    services.AddHostedService<Worker>();
                })
                // Configure logging
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    // Add console logging
                    configLogging.AddConsole();
                });

            // Run the host as a console application
            await builder.RunConsoleAsync();
        }
    }
}