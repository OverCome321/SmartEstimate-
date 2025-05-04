using Bl.Interfaces;
using Entities;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class RegisterViewModel : PropertyChangedBase
    {
        #region Fields
        private readonly IUserBL _userBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;
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
        #endregion
        #region Commands
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        #endregion
        public RegisterViewModel(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser)
        {
            _userBL = userBL;
            _navigationService = navigationService;
            _currentUser = currentUser;

            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanRegister);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            ErrorVisibility = Visibility.Collapsed;
        }
        private async Task RegisterAsync()
        {
            ErrorVisibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }
            if (Password != ConfirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }
            try
            {
                if (await _userBL.ExistsAsync(Email))
                {
                    ShowError("Пользователь с таким email уже существует");
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

                await _userBL.AddOrUpdateAsync(user);
                _currentUser.SetUser(user);

                MessageBox.Show("Регистрация успешно завершена! Теперь вы можете войти в систему.",
                    "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigateToLogin();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при регистрации: {ex.Message}");
            }
        }

        private bool CanRegister() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword);

        private void NavigateToLogin() => _navigationService.NavigateTo<LoginPage>();

        private void ShowError(string message)
        {
            ErrorMessage = message;
            ErrorVisibility = Visibility.Visible;
        }
    }
}