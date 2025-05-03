using Bl.DI;
using Bl.Interfaces;
using Dal.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
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

            services.AddSingleton<MainWindow>();

            services.AddSingleton<CurrentUser>(provider => CurrentUser.Instance);

            services.AddScoped<LoginPage>();
            services.AddScoped<RegisterPage>();

            services.AddScoped<INavigationService>(provider =>
            {
                var mainWindow = provider.GetService<MainWindow>();
                return new NavigationService(provider, mainWindow.MainFrame);
            });

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }
    }
}