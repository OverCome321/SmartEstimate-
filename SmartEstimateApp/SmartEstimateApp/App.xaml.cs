using Bl.DI;
using Bl.Interfaces;
using Dal.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.IO;
using System.Windows;

namespace SmartEstimateApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = BuildConfiguration();
            ServiceProvider = ConfigureServices(configuration);

            var userBL = ServiceProvider.GetService<IUserBL>();
            var currentUser = ServiceProvider.GetService<CurrentUser>();
            var credentialsManager = ServiceProvider.GetService<CredentialsManager>();

            //if (await TryAutoLoginAsync(userBL, currentUser, credentialsManager))
            //{
            //    var homeWindow = ServiceProvider.GetService<HomeWindow>();
            //    homeWindow.Show();
            //}
            //else
            //{
            //    var mainWindow = ServiceProvider.GetService<MainWindow>();
            //    mainWindow?.Show();
            //}
            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        private IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

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
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<HomeWindow>();
            services.AddSingleton<CurrentUser>(provider => CurrentUser.Instance);
            services.AddSingleton<CredentialsManager>();

            services.AddScoped<LoginPage>();
            services.AddScoped<RegisterPage>();

            services.AddScoped<INavigationService>(provider =>
            {
                var mainWindow = provider.GetService<MainWindow>();
                return new NavigationService(provider, mainWindow.MainFrame);
            });

            return services.BuildServiceProvider();
        }

        private async Task<bool> TryAutoLoginAsync(IUserBL userBL, CurrentUser currentUser, CredentialsManager credentialsManager)
        {
            try
            {
                var (email, password, isValid) = credentialsManager.LoadCredentials();
                if (!isValid || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    return false;

                var user = await userBL.VerifyPasswordAsync(email, password);
                if (user == null)
                {
                    credentialsManager.ClearCredentials();
                    return false;
                }

                currentUser.SetUser(user);
                return true;
            }
            catch (Exception)
            {
                credentialsManager.ClearCredentials();
                return false;
            }
        }
    }
}