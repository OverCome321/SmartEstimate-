using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ClientsPage : Page
    {
        private readonly ClientsViewModel _viewModel;

        public ClientsPage(IClientBL clientBL, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel, INavigationService navigationService)
        {
            InitializeComponent();
            _viewModel = new ClientsViewModel(clientBL, currentUser, homeWindowViewModel, navigationService);
            this.DataContext = _viewModel;
        }
    }
}
