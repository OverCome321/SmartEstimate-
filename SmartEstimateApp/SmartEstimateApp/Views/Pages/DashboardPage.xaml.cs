using SmartEstimateApp.Models;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage(CurrentUser user)
        {
            InitializeComponent();
            TitleText.Text = $"Добрый день, {user.Email}";
        }
    }
}
