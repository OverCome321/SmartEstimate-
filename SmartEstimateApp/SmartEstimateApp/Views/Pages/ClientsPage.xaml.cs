using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ClientsPage : Page
    {
        public ClientsPage(IClientBL clientBL, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel, INavigationService navigationService)
        {
            InitializeComponent();
            this.DataContext = new ClientsViewModel(clientBL, currentUser, homeWindowViewModel, navigationService);
        }
    }
}
