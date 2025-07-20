using Bl.Interfaces;
using Common.Search;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Input;
using UI.Helpers;
using Client = SmartEstimateApp.Models.Client;

namespace SmartEstimateApp.ViewModels
{
    public class ClientsViewModel : BasePaginatedViewModel<Client>
    {
        private readonly IClientBL _clientBL;
        private readonly CurrentUser _currentUser;
        private readonly HomeWindowViewModel _homeWindowViewModel;
        private readonly INavigationService _navigationService;

        public ICommand DetailsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddNewClientCommand { get; }

        public ClientsViewModel(
            IClientBL clientBL,
            CurrentUser currentUser,
            HomeWindowViewModel homeWindowViewModel,
            INavigationService navigationService)
        {
            _clientBL = clientBL ?? throw new ArgumentNullException(nameof(clientBL));
            _currentUser = currentUser;
            _homeWindowViewModel = homeWindowViewModel;
            _navigationService = navigationService;

            DetailsCommand = new RelayCommand(obj => OnDetails(obj as Client), obj => obj != null);
            DeleteCommand = new RelayCommand(obj => OnDelete(obj as Client), obj => obj != null);
            AddNewClientCommand = new RelayCommand(_ => OnAddNewClient());

            AppMessenger.RegisterForEntityUpdate<Client>(OnClientUpdated);
            AppMessenger.RegisterForEntityDelete<Client>(OnClientDeleted);

            Task.Run(LoadItemsAsync);
        }

        protected override async Task LoadItemsAsync()
        {
            try
            {
                var searchParams = new ClientSearchParams
                {
                    UserId = _currentUser.User.Id,
                    StartIndex = (CurrentPage - 1) * PageSize,
                    ObjectsCount = PageSize
                };

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    searchParams.Name = SearchText;
                    searchParams.Email = SearchText;
                    searchParams.Phone = SearchText;
                }
                else _homeWindowViewModel.ShowLoading();

                var result = await _clientBL.GetAsync(searchParams, includeRelated: true);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Items.Clear();
                    foreach (var c in result.Objects)
                        Items.Add(Mapper.ToModel(c));

                    TotalCount = result.Total;
                    TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    UpdatePageNumbers();
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(ResultsInfo));
                    OnPropertyChanged(nameof(PageInfo));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading clients: {ex.Message}");
            }
            finally
            {
                _homeWindowViewModel.HideLoading();
            }
        }

        private void OnDetails(Client client)
        {
            if (client == null)
                return;

            _navigationService.NavigateTo<ClientsEditPage>(client);
        }

        private async void OnDelete(Client client)
        {
            if (client == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить клиента \"{client.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _homeWindowViewModel.ShowLoading();
            try
            {
                var deleted = await _clientBL.DeleteAsync(client.Id);
                if (deleted)
                {
                    AppMessenger.SendEntityDeleteMessage<Client>(client.Id);
                    _homeWindowViewModel.ShowSuccess("Клиент успешно удален!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _homeWindowViewModel.ShowError("Ошибка при удалении клиента!");
            }
            finally
            {
                _homeWindowViewModel.HideLoading();
            }
        }

        private void OnAddNewClient()
        {
            _navigationService.NavigateTo<ClientsEditPage>();
        }


        private void OnClientUpdated(Client updatedClient)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var existingClient = Items.FirstOrDefault(c => c.Id == updatedClient.Id);
                if (existingClient != null)
                {
                    var index = Items.IndexOf(existingClient);
                    Items[index] = updatedClient;
                }
                else
                {
                    Items.Insert(0, updatedClient);
                    TotalCount++;
                    UpdatePageNumbers();
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(ResultsInfo));
                    OnPropertyChanged(nameof(PageInfo));
                }
            });
        }

        private void OnClientDeleted(long deletedClientId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var clientToRemove = Items.FirstOrDefault(c => c.Id == deletedClientId);
                if (clientToRemove != null)
                {
                    Items.Remove(clientToRemove);
                    TotalCount--;
                    UpdatePageNumbers();
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(ResultsInfo));
                    OnPropertyChanged(nameof(PageInfo));
                }
            });
        }
    }
}
