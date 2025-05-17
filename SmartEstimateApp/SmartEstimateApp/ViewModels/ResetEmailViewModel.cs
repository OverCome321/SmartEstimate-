using Entities;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.Views.Pages;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    internal class ResetEmailViewModel : PropertyChangedBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly INavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;
        private string _email;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value, nameof(Email));
        }

        public ICommand SendVerificationCodeCommand { get; }
        public ICommand GoBackCommand { get; }

        public ResetEmailViewModel(MainWindowViewModel mainViewModel, INavigationService navigationService, IServiceProvider serviceProvider)
        {
            _mainViewModel = mainViewModel;
            _navigationService = navigationService;
            _serviceProvider = serviceProvider;

            SendVerificationCodeCommand = new RelayCommand(
                obj => SendCode(),
                obj => !string.IsNullOrWhiteSpace(Email)
            );

            GoBackCommand = new RelayCommand(obj => GoBack());
        }

        private void SendCode()
        {
            try
            {
                if (!Bl.Managers.Validation.IsValidEmail(Email))
                {
                    _mainViewModel.ShowError(ErrorMessages.EmailInvalidFormat);
                    return;
                }

                _mainViewModel.ShowLoading();

                var verificationPage = _serviceProvider.GetRequiredService<VerificationPage>();
                var verificationViewModel = (VerificationPageViewModel)verificationPage.DataContext;

                verificationViewModel.ClearVerificationHandlers();

                verificationViewModel.SetEmail(Email, VerificationPurpose.PasswordReset);

                verificationViewModel.VerificationSuccess += () =>
                {
                    var passwordResetPage = _serviceProvider.GetRequiredService<PasswordResetPage>();
                    var passwordResetViewModel = (PasswordResetViewModel)passwordResetPage.DataContext;
                    passwordResetViewModel.SetEmail(Email);
                    _navigationService.NavigateTo<PasswordResetPage>();

                    verificationViewModel.ClearVerificationHandlers();
                };

                _navigationService.NavigateTo<VerificationPage>();
            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError($"Ошибка при отправке кода: {ex.Message}");
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}