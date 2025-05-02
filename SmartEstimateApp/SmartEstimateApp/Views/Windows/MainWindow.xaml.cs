using Bl.Interfaces;
using Entities;
using System.Windows;

namespace SmartEstimateApp.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IUserBL _userBL;

        public MainWindow(IUserBL userBL)
        {
            _userBL = userBL;
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var newUser = new User
                {
                    Email = "test@example.com",
                    PasswordHash = "Test@12345",
                    Role = new Role { Id = new Guid("DEC9BE83-F363-415D-915D-B373F14788BF"), Name = "Пользователь" },
                    LastLogin = DateTime.Now
                };

                Guid userId = await _userBL.AddOrUpdateAsync(newUser);
                MessageBox.Show($"Пользователь создан. ID: {userId}");

                var user = await _userBL.GetAsync(userId, includeRole: true);
                MessageBox.Show($"Email: {user.Email}, Роль: {user.Role?.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
