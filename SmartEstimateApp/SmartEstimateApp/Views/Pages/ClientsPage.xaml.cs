using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ClientsPage : Page
    {
        public ClientsPage(IClientBL clientBL, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel)
        {
            InitializeComponent();
            this.DataContext = new ClientsViewModel(clientBL, currentUser, homeWindowViewModel);
        }
    }
}
