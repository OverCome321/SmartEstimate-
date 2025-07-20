using Bl.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.Views.Pages;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    internal class PasswordResetViewModel : PropertyChangedBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly INavigationService _navigationService;
        private readonly IUserBL _userBL;

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
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, nameof(Email));
        }


        public ICommand ChangePasswordCommand { get; }

        public ICommand GoBackCommand { get; }

        public PasswordResetViewModel(MainWindowViewModel mainViewModel, INavigationService navigationService, IUserBL userBl)
        {
            _mainViewModel = mainViewModel;
            _navigationService = navigationService;
            _userBL = userBl;

            ChangePasswordCommand = new RelayCommand(
                obj => ChangePassword(),
                obj => !string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ConfirmPassword)
            );

            GoBackCommand = new RelayCommand(obj => GoBack());
        }

        public void SetEmail(string email)
        {
            Email = email;
        }

        private async void ChangePassword()
        {
            _mainViewModel.ShowLoading();
            try
            {
                if (Password != ConfirmPassword)
                {
                    _mainViewModel.ShowError("Пароли не совпадают");
                    return;
                }
                var currentUser = await _userBL.GetAsync(Email, true);
                currentUser.PasswordHash = Password;
                currentUser.LastLogin = DateTime.Now;

                await _userBL.AddOrUpdateAsync(currentUser);

                _mainViewModel.ShowSuccess("Пароль успешно изменен");

                _navigationService.NavigateTo<LoginPage>();

            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError($"Ошибка при регистрации: {ex.Message}");
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private void GoBack()
        {
            _navigationService.NavigateTo<LoginPage>();
        }
    }
}
