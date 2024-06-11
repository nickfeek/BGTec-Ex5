using System.Collections.Generic;
using System.Threading.Tasks;
using AnprFileService.Models;

namespace AnprFileService.Data
{
    // Interface for the data repository
    public interface IDataRepository
    {
        // Ensures that the database and table are created
        Task EnsureDatabaseCreatedAsync();

        // Checks if a file record with the specified relative path exists in the database
        Task<bool> FileRecordExistsAsync(string relativePath);

        // Saves a file record to the database
        Task SaveFileRecordAsync(FileRecord fileRecord);
    }
}
