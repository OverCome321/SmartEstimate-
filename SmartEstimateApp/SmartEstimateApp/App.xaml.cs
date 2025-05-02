using Bl.DI;
using Dal.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Views.Windows;
using System.IO;
using System.Windows;

namespace SmartEstimateApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddBusinessLogic(options =>
            {
                options.EnableExtendedValidation = true;
                options.MinPasswordLength = 10;
                options.RequireComplexPassword = true;
                options.MaxFailedLoginAttempts = 3;
            });

            services.AddDataAccess(configuration);

            services.AddScoped<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
