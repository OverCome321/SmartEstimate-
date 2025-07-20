using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class ProjectsPage : Page
    {
        public ProjectsPage(IProjectBL projectBL, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel, INavigationService navigationService)
        {
            InitializeComponent();
            this.DataContext = new ProjectsViewModel(projectBL, currentUser, homeWindowViewModel, navigationService);
        }
    }
}
