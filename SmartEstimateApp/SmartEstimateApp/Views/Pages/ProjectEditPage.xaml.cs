using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ProjectEditPage : Page, IParameterReceiver
    {
        private readonly ProjectEditViewModel _viewModel;

        public ProjectEditPage(IProjectBL projectBL,
            IClientBL clientBL,
            INavigationService navigationService, CurrentUser currentUser)
        {
            InitializeComponent();
            _viewModel = new ProjectEditViewModel(projectBL, clientBL, navigationService, currentUser);
            this.DataContext = _viewModel;
        }
        public void SetParameter(object parameter)
        {
            if (parameter is Project project)
            {
                _viewModel.LoadProject(project);
            }
        }
    }
}
