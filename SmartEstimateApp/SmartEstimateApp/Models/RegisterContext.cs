using Bl.Interfaces;
using SmartEstimateApp.Interfaces;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Windows;

namespace SmartEstimateApp.Context;

public class RegisterContext : IRegisterContext
{
    public RegisterContext(
        IUserBL userBL,
        INavigationService navigationService,
        CurrentUser currentUser,
        MainWindow mainWindow,
        CredentialsManager credentialsManager,
        MainWindowViewModel mainWindowViewModel,
        IServiceProvider serviceProvider)
    {
        UserBL = userBL;
        NavigationService = navigationService;
        CurrentUser = currentUser;
        MainWindow = mainWindow;
        CredentialsManager = credentialsManager;
        MainWindowViewModel = mainWindowViewModel;
        ServiceProvider = serviceProvider;
    }

    public IUserBL UserBL { get; }
    public INavigationService NavigationService { get; }
    public CurrentUser CurrentUser { get; }
    public MainWindow MainWindow { get; }
    public CredentialsManager CredentialsManager { get; }
    public MainWindowViewModel MainWindowViewModel { get; }
    public IServiceProvider ServiceProvider { get; }
}