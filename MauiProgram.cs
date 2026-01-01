using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using YouSpent.Data;

namespace YouSpent
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "youspent.db3");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register repositories
            builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
            builder.Services.AddScoped<IDayRepository, DayRepository>();
            builder.Services.AddScoped<IMonthRepository, MonthRepository>();
            builder.Services.AddScoped<IYearRepository, YearRepository>();

            // Register services
            builder.Services.AddScoped<ExpenseService>();

            // Register database service
            builder.Services.AddSingleton<DatabaseService>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize database
            Task.Run(async () =>
            {
                using var scope = app.Services.CreateScope();
                var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                await dbService.InitializeAsync();
            }).Wait();

            return app;
        }
    }
}
