using Bl;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Pages
{
    public partial class VerificationPage : Page
    {
        private readonly VerificationPageViewModel _viewModel;

        public VerificationPage(MainWindowViewModel mainViewModel, INavigationService navigationService, EmailVerificationServiceBL emailVerificationService)
        {
            InitializeComponent();
            _viewModel = new VerificationPageViewModel(mainViewModel, navigationService, emailVerificationService);
            DataContext = _viewModel;
        }
        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.VerifyCodeCommand.CanExecute(null))
            {
                _viewModel.VerifyCodeCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}