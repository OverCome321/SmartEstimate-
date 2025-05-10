using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ResetEmailPage : Page
    {
        private readonly ResetEmailViewModel _viewModel;

        public ResetEmailPage(MainWindowViewModel mainViewModel, INavigationService navigationService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = new ResetEmailViewModel(mainViewModel, navigationService, serviceProvider);
            this.DataContext = _viewModel;
        }
        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.SendVerificationCodeCommand.CanExecute(null))
            {
                _viewModel.SendVerificationCodeCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
