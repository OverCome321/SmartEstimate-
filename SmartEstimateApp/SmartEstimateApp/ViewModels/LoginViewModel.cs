using Bl.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class LoginViewModel : PropertyChangedBase
    {
        #region Fields
        private readonly IUserBL _userBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;
        private readonly MainWindow _mainWindow;

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

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value, nameof(ErrorMessage));
        }
        private Visibility _errorVisibility;
        public Visibility ErrorVisibility
        {
            get => _errorVisibility;
            set => SetProperty(ref _errorVisibility, value, nameof(ErrorVisibility));
        }
        private Visibility _loadingVisibility;
        public Visibility LoadingVisibility
        {
            get => _loadingVisibility;
            set => SetProperty(ref _loadingVisibility, value, nameof(LoadingVisibility));
        }
        #endregion
        #region Commands
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        #endregion 
        public LoginViewModel(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow)
        {
            _userBL = userBL;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _mainWindow = mainWindow;


            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            ErrorVisibility = Visibility.Collapsed;
            LoadingVisibility = Visibility.Collapsed;
        }

        private async Task LoginAsync()
        {
            ErrorVisibility = Visibility.Collapsed;
            LoadingVisibility = Visibility.Visible;

            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ShowError("Пожалуйста, заполните все поля");
                    return;
                }

                var user = await _userBL.VerifyPasswordAsync(Email, Password);
                if (user == null)
                {
                    ShowError("Неверный email или пароль");
                    return;
                }
                _currentUser.SetUser(user);
                var homeWindow = new HomeWindow();
                homeWindow.Show();
                _mainWindow.Close();

            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при входе: {ex.Message}");
            }
            finally
            {
                LoadingVisibility = Visibility.Collapsed;
            }
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);

        private void NavigateToRegister() => _navigationService.NavigateTo<RegisterPage>();


        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}