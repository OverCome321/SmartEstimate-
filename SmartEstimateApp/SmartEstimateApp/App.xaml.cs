using Bl.DI;
using Bl.Interfaces;
using Common.Managers;
using Dal.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAIService;
using OpenAIService.Interfaces;
using OpenAIService.Models;
using OpenAIService.Security;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using SmartEstimateApp.Context;
using SmartEstimateApp.Interfaces;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.IO;
using System.Windows;

namespace SmartEstimateApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = BuildConfiguration();
            ServiceProvider = ConfigureServices(configuration);

            AppDomain.CurrentDomain.UnhandledException += (s, exArgs) =>
            {
                var logger = ServiceProvider.GetService<ILogger<App>>();
                logger?.LogCritical(exArgs.ExceptionObject as Exception, "Fatal error (UnhandledException)");
            };

            InitializeHfKey();

            var userBL = ServiceProvider.GetService<IUserBL>();
            var currentUser = ServiceProvider.GetService<CurrentUser>();
            var credentialsManager = ServiceProvider.GetService<CredentialsManager>();

            if (await TryAutoLoginAsync(userBL, currentUser, credentialsManager))
            {
                var homeWindow = ServiceProvider.GetService<HomeWindow>();
                homeWindow.Show();
            }
            else
            {
                var mainWindow = ServiceProvider.GetService<MainWindow>();
                mainWindow?.Show();
            }
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
            string connectionString = ConnectionStringManager.GetConnectionString(configuration);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                )
                .CreateLogger();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            services.AddSingleton<Serilog.ILogger>(Log.Logger);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            services.AddAutoMapper(typeof(Profiles.MappingProfile));

            services.AddBusinessLogic(options =>
            {
                options.EnableExtendedValidation = true;
                options.MinPasswordLength = 10;
                options.RequireComplexPassword = true;
                options.MaxFailedLoginAttempts = 3;
            });

            services.AddDataAccess(configuration);

            // Регистрация окон
            services.AddSingleton<MainWindow>();
            services.AddSingleton<HomeWindow>();

            //Регистрация Managers зависимостей
            services.AddSingleton<CurrentUser>(provider => CurrentUser.Instance);
            services.AddSingleton<CredentialsManager>();

            // Регистрация навигационных сервисов
            services.AddSingleton<INavigationServiceFactory, NavigationServiceFactory>();
            services.AddScoped<INavigationService, NavigationService>();

            //Регистрация ViewModels
            services.AddTransient<ClientsViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<HomeWindowViewModel>();
            services.AddTransient<ClientEditViewModel>();
            services.AddTransient<ProjectsViewModel>();

            // Регистрация страниц
            services.AddScoped<LoginPage>();
            services.AddScoped<RegisterPage>();
            services.AddScoped<VerificationPage>();
            services.AddScoped<PasswordResetPage>();
            services.AddScoped<ResetEmailPage>();
            services.AddTransient<DashboardPage>();
            services.AddTransient<ProjectsPage>();
            services.AddTransient<ClientsPage>();
            services.AddScoped<SettingsPage>();
            services.AddTransient<AnalyticsPage>();
            services.AddTransient<ClientsEditPage>();
            services.AddTransient<ProjectEditPage>();
            services.AddTransient<SupportPage>();


            //Контекст зависимостей
            services.AddScoped<ILoginContext, LoginContext>();
            services.AddScoped<IRegisterContext, RegisterContext>();


            // Регистрация OpenAI блока
            services.Configure<HfSettings>(configuration.GetSection("HuggingFace"));
            services.AddHttpClient<IOpenAiService, HuggingFaceService>();

            return services.BuildServiceProvider();
        }

        private async Task<bool> TryAutoLoginAsync(IUserBL userBL, CurrentUser currentUser, CredentialsManager credentialsManager)
        {
            try
            {
                var (email, password, isValid) = credentialsManager.LoadCredentials();
                if (!isValid || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Invalid or missing credentials");
                    return false;
                }

                var user = await userBL.VerifyPasswordAsync(email, password);
                if (user == null)
                {
                    Console.WriteLine("User verification failed");
                    credentialsManager.ClearCredentials();
                    return false;
                }

                var modelUser = Mapper.ToModel(user);
                currentUser.SetUser(modelUser);
                return true;
            }
            catch
            {
                credentialsManager.ClearCredentials();
                return false;
            }
        }

        private void InitializeHfKey()
        {
            string installDir = AppDomain.CurrentDomain.BaseDirectory;
            string tempHfKeyFile = Path.Combine(installDir, "initial_key.txt");

            if (File.Exists(tempHfKeyFile) && !ApiKeyEncryptor.ApiKeyExists())
            {
                try
                {
                    string rawKey = File.ReadAllText(tempHfKeyFile).Trim();
                    if (!string.IsNullOrEmpty(rawKey))
                    {
                        ApiKeyEncryptor.SaveApiKey(rawKey);
                        File.Delete(tempHfKeyFile);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при инициализации HF-ключа: {ex.Message}");
                }
            }
        }

    }

}
