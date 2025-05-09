using Bl.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class RegisterViewModel : PropertyChangedBase
    {
        #region Fields
        private readonly IUserBL _userBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;
        private readonly MainWindow _mainWindow;
        private readonly CredentialsManager _credentialsManager;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly IServiceProvider _serviceProvider;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, nameof(Email));
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, nameof(Password));
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value, nameof(ConfirmPassword));
        }

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value, nameof(RememberMe));
        }
        #endregion

        #region Commands
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        #endregion

        public RegisterViewModel(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel, IServiceProvider serviceProvider)
        {
            _userBL = userBL;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _credentialsManager = credentialsManager;
            _mainWindowViewModel = mainWindowViewModel;
            _serviceProvider = serviceProvider;

            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanRegister);

            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        private async Task RegisterAsync()
        {
            _mainWindowViewModel.ShowLoading();
            try
            {
                if (Password != ConfirmPassword)
                {
                    _mainWindowViewModel.ShowError("Пароли не совпадают");
                    return;
                }

                if (await _userBL.ExistsAsync(Email))
                {
                    _mainWindowViewModel.ShowError("Пользователь с таким email уже существует");
                    return;
                }

                var user = new User
                {
                    Email = Email,
                    PasswordHash = Password,
                    Role = new Role { Id = 1, Name = "User" },
                    CreatedAt = DateTime.Now,
                    LastLogin = DateTime.Now
                };
                var entityUser = Mapper.ToEntity(user);
                await _userBL.ValidationCommand(entityUser);

                var verificationPage = _serviceProvider.GetRequiredService<VerificationPage>();

                var verificationViewModel = (VerificationPageViewModel)verificationPage.DataContext;

                verificationViewModel.SetEmail(Email);

                verificationViewModel.VerificationSuccess += async () =>
                {
                    try
                    {
                        await _userBL.AddOrUpdateAsync(entityUser);
                        CompleteRegistration(user);
                    }
                    catch (Exception ex)
                    {
                        _mainWindowViewModel.ShowError($"Ошибка при завершении регистрации: {ex.Message}");
                    }
                };
                _navigationService.NavigateTo<VerificationPage>();
            }
            catch (Exception ex)
            {
                _mainWindowViewModel.ShowError($"Ошибка при регистрации: {ex.Message}");
            }
            finally
            {
                _mainWindowViewModel.HideLoading();
            }
        }

        private void CompleteRegistration(User user)
        {
            _currentUser.SetUser(user);
            _credentialsManager.SaveCredentials(Email, Password, RememberMe);

            var homeWindow = new HomeWindow();
            homeWindow.Show();

            _mainWindow.Close();
        }

        private bool CanRegister() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword);

        private void NavigateToLogin() => _navigationService.NavigateTo<LoginPage>();
    }
}