using System.Windows.Controls;

namespace SmartEstimateApp.Navigation.Interfaces
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void NavigateTo<TPage>() where TPage : Page;
        void NavigateTo<TPage>(object parameter) where TPage : Page;
        void NavigateTo(Page page, object parameter = null);
        void GoBack();
        void NavigateAsRoot<TPage>() where TPage : Page;
    }
}
