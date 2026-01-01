using Microsoft.EntityFrameworkCore;

namespace YouSpent.Data
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;

        public DatabaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Create database if it doesn't exist
                await _context.Database.EnsureCreatedAsync();
                
                // Or use migrations (recommended for production)
                // await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ClearAllDataAsync()
        {
            try
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Clear data failed: {ex.Message}");
                return false;
            }
        }

        public string GetDatabasePath()
        {
            return _context.Database.GetConnectionString() ?? string.Empty;
        }
    }
}
