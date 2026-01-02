using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using YouSpent.Data;
using YouSpent.Services;
using YouSpent.Views;
using YouSpent.ViewModels;

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
            
            // Register localization service
            builder.Services.AddSingleton<LocalizationService>();

            // Register database service
            builder.Services.AddSingleton<DatabaseService>();

            // Register ViewModels
            builder.Services.AddTransient<ExpensesPageViewModel>();

            // Register Pages
            builder.Services.AddTransient<ExpensesPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize localization
            var localizationService = app.Services.GetRequiredService<LocalizationService>();
            localizationService.SetDeviceLanguage();

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
