using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows.Controls;
using Entities;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ClientsEditPage : Page
    {
        private readonly ClientEditViewModel _viewModel;

        public ClientsEditPage(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser)
        {
            InitializeComponent();
            _viewModel = new ClientEditViewModel(clientBL, navigationService, currentUser);
            this.DataContext = _viewModel;
        }

        public void SetClient(Client client)
        {
            _viewModel.LoadClient(client);
        }
    }
}