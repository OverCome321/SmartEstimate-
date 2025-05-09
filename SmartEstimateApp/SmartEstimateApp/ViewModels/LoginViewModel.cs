using Bl.Interfaces;
using Bl.Managers;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using SmartEstimateApp.Views.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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
        private readonly IServiceProvider _serviceProvider;

        private string _email;
        private string _password;
        private bool _rememberMe;
        private bool _isLoginCooldownActive;
        private string? _countdownText;
        private DispatcherTimer _loginTimer;
        private DateTime? _lockoutEnd;

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

        public LoginViewModel(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser,
            MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel, IServiceProvider serviceProvider)
        {
            _userBL = userBL;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _mainWindow = mainWindow;
            _credentialsManager = credentialsManager;
            _mainWindowViewModel = mainWindowViewModel;
            _serviceProvider = serviceProvider;

            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            NavigateToPasswordResetCommand = new RelayCommand(NavigateToPasswordReset);
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

                var (canLogin, errorMessage, remainingSeconds) = LoginAttemptStore.CanLogin(Email);
                if (!canLogin)
                {
                    _mainWindowViewModel.ShowError(errorMessage);
                    StartLoginCooldown(remainingSeconds.Value);
                    return;
                }

                var user = await _userBL.VerifyPasswordAsync(Email, Password);
                if (user == null)
                {
                    _mainWindowViewModel.ShowError("Неверный email или пароль");
                    _credentialsManager.ClearCredentials();

                    var (isLockedOut, cooldownSeconds) = LoginAttemptStore.RecordFailedAttempt(Email);
                    if (isLockedOut)
                    {
                        _mainWindowViewModel.ShowError($"Слишком много попыток. Попробуйте снова через {cooldownSeconds} сек.");
                        StartLoginCooldown(cooldownSeconds.Value);
                    }
                    return;
                }

                LoginAttemptStore.ResetAttempts(Email);

                var verificationPage = _serviceProvider.GetRequiredService<VerificationPage>();
                _navigationService.NavigateTo<VerificationPage>();

                var verificationViewModel = (VerificationPageViewModel)verificationPage.DataContext;
                verificationViewModel.SetEmail(Email);

                var modelUser = Mapper.ToModel(user);
                verificationViewModel.VerificationSuccess += () => CompleteLogin(modelUser);
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

        private bool CanLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return false;

            var (canLogin, _, _) = LoginAttemptStore.CanLogin(Email);
            return canLogin;
        }

        private void NavigateToRegister() => _navigationService.NavigateTo<RegisterPage>();

        private void NavigateToPasswordReset() => _navigationService.NavigateTo<ResetEmailPage>();

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

        private void CompleteLogin(User user)
        {
            _currentUser.SetUser(user);
            _credentialsManager.SaveCredentials(Email, Password, RememberMe);

            var homeWindow = new HomeWindow();
            homeWindow.Show();

            _mainWindow.Close();
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