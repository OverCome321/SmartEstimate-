using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Interfaces;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class RegisterViewModel : PropertyChangedBase
    {
        #region Fields
        private readonly IRegisterContext _ctx;

        private string _storedPassword;

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

        public RegisterViewModel(IRegisterContext ctx)
        {
            _ctx = ctx;

            RegisterCommand = new RelayCommand(async () => await RegisterAsync(), CanRegister);

            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
        }

        private async Task RegisterAsync()
        {
            _ctx.MainWindowViewModel.ShowLoading();
            try
            {
                if (Password != ConfirmPassword)
                {
                    _ctx.MainWindowViewModel.ShowError("Пароли не совпадают");
                    return;
                }

                if (await _ctx.UserBL.ExistsAsync(Email))
                {
                    _ctx.MainWindowViewModel.ShowError("Пользователь с таким email уже существует");
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
                await _ctx.UserBL.ValidationCommand(entityUser);

                var verificationPage = _ctx.ServiceProvider.GetRequiredService<VerificationPage>();

                var verificationViewModel = (VerificationPageViewModel)verificationPage.DataContext;

                verificationViewModel.SetEmail(Email);

                verificationViewModel.VerificationSuccess += async () =>
                {
                    try
                    {
                        await _ctx.UserBL.AddOrUpdateAsync(entityUser);
                        CompleteRegistration(user);
                    }
                    catch (Exception ex)
                    {
                        _ctx.MainWindowViewModel.ShowError($"Ошибка при завершении регистрации: {ex.Message}");
                    }
                };
                _storedPassword = Password;
                _ctx.NavigationService.NavigateTo<VerificationPage>();
            }
            catch (Exception ex)
            {
                _ctx.MainWindowViewModel.ShowError($"Ошибка при регистрации: {ex.Message}");
            }
            finally
            {
                _ctx.MainWindowViewModel.HideLoading();
            }
        }

        private void CompleteRegistration(User user)
        {
            _ctx.CurrentUser.SetUser(user);
            _ctx.CredentialsManager.SaveCredentials(Email, _storedPassword, RememberMe);

            var homeWindow = new HomeWindow(_ctx.ServiceProvider);
            homeWindow.Show();

            _ctx.MainWindow.Close();
        }

        private bool CanRegister() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(ConfirmPassword);

        private void NavigateToLogin() => _ctx.NavigationService.NavigateTo<LoginPage>();
    }
}