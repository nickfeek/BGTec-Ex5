using Microsoft.EntityFrameworkCore;
using AnprFileService.Models;

namespace AnprFileService.Data
{
    // DbContext class for interacting with the database
    public class AppDbContext : DbContext
    {
        // Constructor that accepts DbContextOptions
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet representing the 'Files' table in the database
        public DbSet<FileRecord> Files { get; set; }

        // Configure model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure unique constraint for 'Path' column. This ensures that a file cannot be processes mulitple times
            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => f.Path)
                .IsUnique();

            // Create an index on the DateTime column
            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => f.Date);

            // Create an index on the DateTime column
            modelBuilder.Entity<FileRecord>()
                .HasIndex(f => f.CameraName);

        }
    }
}
