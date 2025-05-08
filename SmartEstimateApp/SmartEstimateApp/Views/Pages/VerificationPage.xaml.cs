using Bl;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class VerificationPage : Page
    {
        public VerificationPage(MainWindowViewModel mainViewModel, INavigationService navigationService, EmailVerificationServiceBL emailVerificationService)
        {
            InitializeComponent();
            DataContext = new VerificationPageViewModel(mainViewModel, navigationService, emailVerificationService);
        }
    }
}