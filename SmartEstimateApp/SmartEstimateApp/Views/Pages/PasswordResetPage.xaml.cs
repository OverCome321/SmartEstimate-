using Bl.Interfaces;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class PasswordResetPage : Page
    {
        private readonly PasswordResetViewModel _viewModel;

        public PasswordResetPage(MainWindowViewModel mainViewModel, INavigationService navigationService, IUserBL userBl)
        {
            InitializeComponent();
            _viewModel = new PasswordResetViewModel(mainViewModel, navigationService, userBl);
            DataContext = _viewModel;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
                var pb = (PasswordBox)sender;
                pb.Tag = string.IsNullOrEmpty(pb.Password) ? "" : "*";
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
                var pb = (PasswordBox)sender;
                pb.Tag = string.IsNullOrEmpty(pb.Password) ? "" : "*";
            }
        }
    }
}
