using Bl.Interfaces;
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
            _viewModel.LoadClient(parameter as Entities.Client);
        }
    }
}