using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ResetEmailPage : Page
    {
        public ResetEmailPage(MainWindowViewModel mainViewModel, INavigationService navigationService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            this.DataContext = new ResetEmailViewModel(mainViewModel, navigationService, serviceProvider);
        }
    }
}
