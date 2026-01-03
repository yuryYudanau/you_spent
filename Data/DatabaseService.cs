using Microsoft.EntityFrameworkCore;
using YouSpent.Models;

namespace YouSpent.Data
{
    public class DatabaseService
    {
        private readonly AppDbContext _context;
        private static readonly SemaphoreSlim _initLock = new(1, 1);
        private static bool _isInitialized;

        public DatabaseService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Initializes database using migrations approach (thread-safe)
        /// </summary>
        public async Task InitializeAsync()
        {
            await _initLock.WaitAsync();
            
            try
            {
                if (_isInitialized)
                {
                    System.Diagnostics.Debug.WriteLine("[DB] Already initialized, skipping");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("[DB] Starting initialization...");
                
                // Check if database needs recreation due to schema conflicts
                var needsRecreation = await CheckIfDatabaseNeedsRecreationAsync();
                
                if (needsRecreation)
                {
                    System.Diagnostics.Debug.WriteLine("[DB] Schema conflict detected, recreating database...");
                    await RecreateDatabaseAsync();
                }
                
                // Apply pending migrations
                await _context.Database.MigrateAsync();
                
                System.Diagnostics.Debug.WriteLine("[DB] Migrations applied");
                
                // Seed default data after migrations
                await SeedDefaultExpenseTypesAsync();
                
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("[DB] Initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Migration failed: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[DB] Message: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB] Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                
                // Try to recover by recreating database
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DB] Attempting to recover by recreating database...");
                    await RecreateDatabaseAsync();
                    await _context.Database.MigrateAsync();
                    await SeedDefaultExpenseTypesAsync();
                    _isInitialized = true;
                    System.Diagnostics.Debug.WriteLine("[DB] Recovery successful");
                    return;
                }
                catch (Exception recoveryEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB] Recovery failed: {recoveryEx.Message}");
                }
                
                // On Android, log but don't crash the app
#if ANDROID
                System.Diagnostics.Debug.WriteLine("[DB] Android: Continuing despite error");
#else
                throw new InvalidOperationException("Failed to initialize database. Check migration files.", ex);
#endif
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Checks if database needs recreation due to schema conflicts
        /// </summary>
        private async Task<bool> CheckIfDatabaseNeedsRecreationAsync()
        {
            try
            {
                // Check if migrations history exists
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                    return false;

                // Check for pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                // If we have applied migrations but ExpenseTypes table doesn't exist, recreate
                if (appliedMigrations.Any())
                {
                    try
                    {
                        await _context.ExpenseTypes.AnyAsync();
                        return false; // Table exists, no need to recreate
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("[DB] ExpenseTypes table missing despite applied migrations");
                        return true; // Table missing, need recreation
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Check recreation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Recreates database from scratch
        /// </summary>
        private async Task RecreateDatabaseAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DB] Deleting old database...");
                await _context.Database.EnsureDeletedAsync();
                
                System.Diagnostics.Debug.WriteLine("[DB] Old database deleted");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Failed to delete database: {ex.Message}");
            }
        }

        /// <summary>
        /// Seeds default expense types if none exist
        /// </summary>
        private async Task SeedDefaultExpenseTypesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DB] Checking expense types...");
                
                var count = await _context.ExpenseTypes.CountAsync();
                
                if (count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[DB] Seeding default types...");
                    
                    var defaultTypes = new[]
                    {
                        new ExpenseType { Name = "Food", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Transport", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Entertainment", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Shopping", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Bills", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Healthcare", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Education", IsActive = true, CreatedAt = DateTime.Now },
                        new ExpenseType { Name = "Other", IsActive = true, CreatedAt = DateTime.Now }
                    };

                    await _context.ExpenseTypes.AddRangeAsync(defaultTypes);
                    await _context.SaveChangesAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"[DB] Seeded {defaultTypes.Length} types successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DB] Found {count} existing types, skipping seed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Seeding failed: {ex.GetType().Name}: {ex.Message}");
                // Don't throw - seeding is not critical
            }
        }

        /// <summary>
        /// Gets list of pending migrations
        /// </summary>
        public async Task<IEnumerable<string>> GetPendingMigrationsAsync()
        {
            try
            {
                return await _context.Database.GetPendingMigrationsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetPending failed: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets list of applied migrations
        /// </summary>
        public async Task<IEnumerable<string>> GetAppliedMigrationsAsync()
        {
            try
            {
                return await _context.Database.GetAppliedMigrationsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetApplied failed: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Clears all data from database (for development/testing only)
        /// </summary>
        public async Task<bool> ClearAllDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DB] Clearing all data...");
                
                // Delete all records but keep schema
                await _context.Expenses.ExecuteDeleteAsync();
                await _context.Days.ExecuteDeleteAsync();
                await _context.Weeks.ExecuteDeleteAsync();
                await _context.Months.ExecuteDeleteAsync();
                await _context.Years.ExecuteDeleteAsync();
                await _context.ExpenseTypes.ExecuteDeleteAsync();
                
                System.Diagnostics.Debug.WriteLine("[DB] Data cleared");
                
                // Re-seed default data
                await SeedDefaultExpenseTypesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] Clear failed: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets database connection string/path
        /// </summary>
        public string GetDatabasePath()
        {
            try
            {
                return _context.Database.GetConnectionString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] GetPath failed: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if database can be connected to
        /// </summary>
        public async Task<bool> CanConnectAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB] CanConnect failed: {ex.Message}");
                return false;
            }
        }
    }
}
