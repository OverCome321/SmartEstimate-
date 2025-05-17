using Bl.Managers;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Interfaces;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartEstimateApp.ViewModels
{
    public class LoginViewModel : PropertyChangedBase
    {
        private readonly ILoginContext _ctx;

        private string _email;
        private string _password;
        private bool _rememberMe;
        private bool _isLoginCooldownActive;
        private string? _countdownText;
        private DispatcherTimer _loginTimer;
        private DateTime? _lockoutEnd;
        private string _storedPassword;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, nameof(Email));
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value, nameof(Password));
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value, nameof(RememberMe));
        }

        public bool IsLoginCooldownActive
        {
            get => _isLoginCooldownActive;
            set => SetProperty(ref _isLoginCooldownActive, value, nameof(IsLoginCooldownActive));
        }

        public string? CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value, nameof(CountdownText));
        }

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand NavigateToPasswordResetCommand { get; }

        public LoginViewModel(ILoginContext ctx)
        {
            _ctx = ctx;

            LoginCommand = new RelayCommand(async obj => await LoginAsync(), obj => CanLogin());
            NavigateToRegisterCommand = new RelayCommand(obj => NavigateToRegister());
            NavigateToPasswordResetCommand = new RelayCommand(obj => NavigateToPasswordReset());
            LoadCredentials();
        }

        private async Task LoginAsync()
        {
            _ctx.MainWindowViewModel.ShowLoading();
            try
            {
                var (canLogin, errorMessage, remainingSeconds) = LoginAttemptStore.CanLogin(Email);
                if (!canLogin)
                {
                    _ctx.MainWindowViewModel.ShowError(errorMessage);
                    StartLoginCooldown(remainingSeconds.Value);
                    return;
                }

                var user = await _ctx.UserBL.VerifyPasswordAsync(Email, Password);
                if (user == null)
                {
                    _ctx.MainWindowViewModel.ShowError("Неверный email или пароль");
                    _ctx.CredentialsManager.ClearCredentials();

                    var (isLockedOut, cooldownSeconds) = LoginAttemptStore.RecordFailedAttempt(Email);
                    if (isLockedOut)
                    {
                        _ctx.MainWindowViewModel.ShowError($"Слишком много попыток. Попробуйте снова через {cooldownSeconds} сек.");
                        StartLoginCooldown(cooldownSeconds.Value);
                    }
                    return;
                }

                LoginAttemptStore.ResetAttempts(Email);

                var verificationPage = _ctx.ServiceProvider.GetRequiredService<VerificationPage>();
                var verificationViewModel = (VerificationPageViewModel)verificationPage.DataContext;

                verificationViewModel.ClearVerificationHandlers();

                verificationViewModel.SetEmail(Email, VerificationPurpose.Login);

                var modelUser = Mapper.ToModel(user);
                verificationViewModel.VerificationSuccess += () =>
                {
                    CompleteLogin(modelUser);

                    verificationViewModel.ClearVerificationHandlers();
                };
                _storedPassword = Password;
                _ctx.NavigationService.NavigateTo<VerificationPage>();
            }
            catch (Exception ex)
            {
                _ctx.MainWindowViewModel.ShowError($"Ошибка при входе: {ex.Message}");
                _ctx.CredentialsManager.ClearCredentials();
            }
            finally
            {
                _ctx.MainWindowViewModel.HideLoading();
            }
        }

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return false;

            var (canLogin, _, _) = LoginAttemptStore.CanLogin(Email);
            return canLogin;
        }

        private void NavigateToRegister() => _ctx.NavigationService.NavigateTo<RegisterPage>();

        private void NavigateToPasswordReset() => _ctx.NavigationService.NavigateTo<ResetEmailPage>();

        private void LoadCredentials()
        {
            var (email, password, isValid) = _ctx.CredentialsManager.LoadCredentials();
            if (isValid)
            {
                Email = email;
                RememberMe = true;
            }
        }

        private void CompleteLogin(Models.User user)
        {
            _ctx.CurrentUser.SetUser(user);
            _ctx.CredentialsManager.SaveCredentials(Email, _storedPassword, RememberMe);

            var homeWindow = new HomeWindow(_ctx.ServiceProvider, _ctx.HomeWindowViewModel);
            homeWindow.Show();

            _ctx.MainWindow.Close();
        }

        private void StartLoginCooldown(int seconds)
        {
            IsLoginCooldownActive = true;
            _lockoutEnd = DateTime.UtcNow.AddSeconds(seconds);

            if (_loginTimer != null && _loginTimer.IsEnabled)
            {
                _loginTimer.Stop();
            }

            _loginTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _loginTimer.Tick += (s, e) =>
            {
                var timeLeft = _lockoutEnd.Value - DateTime.UtcNow;
                if (timeLeft.TotalSeconds <= 0)
                {
                    _loginTimer.Stop();
                    IsLoginCooldownActive = false;
                    CountdownText = null;
                }
                else
                {
                    CountdownText = $"Попробуйте снова через {(int)Math.Ceiling(timeLeft.TotalSeconds)} сек.";
                }
            };

            _loginTimer.Start();
        }
    }
}