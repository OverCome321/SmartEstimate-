using Bl.Interfaces;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Windows;

namespace SmartEstimateApp.Interfaces;

public interface IRegisterContext
{
    IUserBL UserBL { get; }
    INavigationService NavigationService { get; }
    CurrentUser CurrentUser { get; }
    MainWindow MainWindow { get; }
    CredentialsManager CredentialsManager { get; }
    MainWindowViewModel MainWindowViewModel { get; }
    IServiceProvider ServiceProvider { get; }
}
