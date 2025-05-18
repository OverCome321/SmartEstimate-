using Bl.Interfaces;
using Entities;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ClientsEditPage : Page, IParameterReceiver
    {
        private readonly ClientEditViewModel _viewModel;

        public ClientsEditPage(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser)
        {
            InitializeComponent();
            _viewModel = new ClientEditViewModel(clientBL, navigationService, currentUser);
            this.DataContext = _viewModel;
        }

        public void SetParameter(object parameter)
        {
            if (parameter is Client client)
            {
                _viewModel.LoadClient(client);
            }
        }
    }
}