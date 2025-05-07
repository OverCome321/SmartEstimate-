using Bl.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class LoginViewModel : PropertyChangedBase
    {
        private readonly IUserBL _userBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;
        private readonly MainWindow _mainWindow;
        private readonly CredentialsManager _credentialsManager;
        private readonly MainWindowViewModel _mainWindowViewModel;

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

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value, nameof(RememberMe));
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel)
        {
            _userBL = userBL;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _credentialsManager = credentialsManager;
            _mainWindowViewModel = mainWindowViewModel;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            LoadCredentials();
        }

        private async Task LoginAsync()
        {
            _mainWindowViewModel.ShowLoading();
            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    _mainWindowViewModel.ShowError("Пожалуйста, заполните все поля");
                    return;
                }

                var user = await _userBL.VerifyPasswordAsync(Email, Password);
                if (user == null)
                {
                    _mainWindowViewModel.ShowError("Неверный email или пароль");
                    _credentialsManager.ClearCredentials();
                    return;
                }

                _currentUser.SetUser(user);
                _credentialsManager.SaveCredentials(Email, Password, RememberMe);
                var homeWindow = new HomeWindow();
                homeWindow.Show();
                _mainWindow.Close();
            }
            catch (Exception ex)
            {
                _mainWindowViewModel.ShowError($"Ошибка при входе: {ex.Message}");
                _credentialsManager.ClearCredentials();
            }
            finally
            {
                _mainWindowViewModel.HideLoading();
            }
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        private void NavigateToRegister() => _navigationService.NavigateTo<RegisterPage>();

        private void LoadCredentials()
        {
            var (email, password, isValid) = _credentialsManager.LoadCredentials();
            if (isValid)
            {
                Email = email;
                Password = password;
                RememberMe = true;
            }
        }
    }
}