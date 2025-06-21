using Bl.Interfaces;
using Entities;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using System.Collections.ObjectModel;
using System.Linq; // <-- Добавлено
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class ClientEditViewModel : PropertyChangedBase
    {
        private readonly IClientBL _clientBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;

        private Client _client;
        public Client Client
        {
            get => _client;
            set => SetProperty(ref _client, value, nameof(Client));
        }

        // --- ИЗМЕНЕНО: Тип коллекции изменен со string на StatusOption ---
        private ObservableCollection<StatusOption> _statusOptions;
        public ObservableCollection<StatusOption> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value, nameof(StatusOptions));
        }
        // -----------------------------------------------------------------

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientEditViewModel(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser)
        {
            _clientBL = clientBL;
            _navigationService = navigationService;
            _currentUser = currentUser;

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => _navigationService.GoBack());

            LoadStatusOptions();
        }

        // --- ИЗМЕНЕНО: Метод теперь создает объекты StatusOption ---
        private void LoadStatusOptions()
        {
            StatusOptions = new ObservableCollection<StatusOption>
            {
                new StatusOption { Id = 1, Name = "Активен" },
                new StatusOption { Id = 2, Name = "Неактивен" }
            };
        }
        // ---------------------------------------------------------

        public void LoadClient(Client client)
        {
            // --- ИЗМЕНЕНО: Логика загрузки и создания клиента под int статус ---
            Client = client != null ? new Client
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                Address = client.Address,
                // Если у существующего клиента статус не задан (равен 0), ставим по умолчанию 1 ("Активен")
                Status = client.Status > 0 ? client.Status : 1,
                CreatedAt = client.CreatedAt,
                User = client.User ?? Mapper.ToEntity(_currentUser.User)
            } : new Client
            {
                User = Mapper.ToEntity(_currentUser.User),
                // Для нового клиента ставим статус по умолчанию "Активен"
                Status = 1
            };
            // ----------------------------------------------------------------------
        }

        private bool CanSave()
        {
            // Добавим проверку статуса для надежности
            return Client != null && !string.IsNullOrWhiteSpace(Client.Name) && Client.Status > 0;
        }

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля (*) и выберите статус.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Client.User == null)
                    Client.User = Mapper.ToEntity(_currentUser.User);

                await _clientBL.AddOrUpdateAsync(Client);
                _navigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}