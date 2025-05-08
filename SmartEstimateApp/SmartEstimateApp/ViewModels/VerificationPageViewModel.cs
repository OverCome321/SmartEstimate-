using Bl;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class VerificationPageViewModel : PropertyChangedBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly INavigationService _navigationService;
        private readonly EmailVerificationServiceBL _emailVerificationService;

        private string _verificationCode;

        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value, nameof(VerificationCode));
        }

        public ICommand VerifyCodeCommand { get; }

        public ICommand CancelVerificationCommand { get; }

        public event Action VerificationSuccess;

        private string _email;

        public VerificationPageViewModel(MainWindowViewModel mainViewModel, INavigationService navigationService, EmailVerificationServiceBL emailVerificationService)
        {
            _mainViewModel = mainViewModel;
            _navigationService = navigationService;

            _emailVerificationService = emailVerificationService;
            VerifyCodeCommand = new RelayCommand(async () => VerifyCodeAsync(),
                () => !string.IsNullOrWhiteSpace(VerificationCode));
            CancelVerificationCommand = new RelayCommand(CancelVerification);
        }

        public void SetEmail(string email)
        {
            _email = email;
        }
        private void VerifyCodeAsync()
        {
            try
            {
                _mainViewModel.ShowLoading();

                if (_emailVerificationService.VerifyCode(_email, VerificationCode))
                {
                    VerificationSuccess?.Invoke();
                }
                else
                {
                    _mainViewModel.ShowError("Неверный код подтверждения");
                }
            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError($"Ошибка при проверке кода: {ex.Message}");
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private void CancelVerification()
        {
            _navigationService.GoBack();
        }
    }
}