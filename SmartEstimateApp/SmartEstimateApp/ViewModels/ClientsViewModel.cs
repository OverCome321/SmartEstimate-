using Bl.Interfaces;
using Common.Search;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Views.Pages;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class ClientsViewModel : PropertyChangedBase
    {
        private readonly IClientBL _clientBL;
        private readonly CurrentUser _currentUser;
        private readonly HomeWindowViewModel _homeWindowViewModel;

        // Only current page clients
        public ObservableCollection<Client> Clients { get; set; } = new();

        // Search
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value, nameof(SearchText)))
                {
                    CurrentPage = 1;
                    Task.Run(LoadClientsAsync);
                }
            }
        }

        // Pagination Properties
        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value, nameof(PageSize)))
                {
                    CurrentPage = 1;
                    Task.Run(LoadClientsAsync);
                }
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value, nameof(CurrentPage)))
                {
                    Task.Run(LoadClientsAsync);
                }
            }
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value, nameof(TotalPages)))
                {
                    UpdatePageNumbers();
                    OnPropertyChanged(nameof(PageInfo));
                }
            }
        }

        private int _totalCount = 0;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value, nameof(TotalCount)))
                {
                    OnPropertyChanged(nameof(ResultsInfo));
                }
            }
        }

        public bool HasResults => Clients.Any();

        public string ResultsInfo => $"Найдено {TotalCount} клиентов";
        public string PageInfo => $"Страница {CurrentPage} из {TotalPages}";

        public List<int> PageSizeOptions { get; } = new List<int> { 10, 20, 50, 100 };

        private ObservableCollection<int> _pageNumbers = new();
        public ObservableCollection<int> PageNumbers
        {
            get => _pageNumbers;
            set => SetProperty(ref _pageNumbers, value, nameof(PageNumbers));
        }

        // Commands
        public ICommand DetailsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddNewClientCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }

        public ClientsViewModel(IClientBL clientBL, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel)
        {
            _clientBL = clientBL ?? throw new ArgumentNullException(nameof(clientBL));
            _currentUser = currentUser;
            _homeWindowViewModel = homeWindowViewModel;
            // Initialize commands
            DetailsCommand = new RelayCommand(obj => OnDetails(obj as Client), obj => CanShowDetails(obj));
            DeleteCommand = new RelayCommand(obj => OnDelete(obj as Client), obj => CanDelete(obj));
            AddNewClientCommand = new RelayCommand(obj => OnAddNewClient());
            ClearSearchCommand = new RelayCommand(obj => OnClearSearch());
            PreviousPageCommand = new RelayCommand(obj => OnPreviousPage(), obj => CanGoToPreviousPage());
            NextPageCommand = new RelayCommand(obj => OnNextPage(), obj => CanGoToNextPage());
            GoToPageCommand = new RelayCommand(obj => OnGoToPage(Convert.ToInt32(obj)));
            GoToFirstPageCommand = new RelayCommand(obj => OnGoToFirstPage(), obj => CanGoToPreviousPage());
            GoToLastPageCommand = new RelayCommand(obj => OnGoToLastPage(), obj => CanGoToNextPage());

            // Load first page of clients
            Task.Run(LoadClientsAsync);
        }

        private async Task LoadClientsAsync()
        {

            try
            {
                var searchParams = new ClientSearchParams
                {
                    UserId = _currentUser.User.Id,
                    StartIndex = (CurrentPage - 1) * PageSize,
                    ObjectsCount = PageSize
                };

                // Поиск по всем полям
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
                    Clients.Clear();
                    foreach (var c in result.Objects)
                        Clients.Add(c);

                    // total count для вычисления страниц
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

        private void UpdatePageNumbers()
        {
            PageNumbers.Clear();

            const int maxVisiblePages = 5;
            int start, end;

            if (TotalPages <= maxVisiblePages)
            {
                start = 1;
                end = TotalPages;
            }
            else if (CurrentPage <= 3)
            {
                start = 1;
                end = maxVisiblePages;
            }
            else if (CurrentPage >= TotalPages - 2)
            {
                start = TotalPages - maxVisiblePages + 1;
                end = TotalPages;
            }
            else
            {
                start = CurrentPage - 2;
                end = CurrentPage + 2;
            }

            for (int i = start; i <= end; i++)
                PageNumbers.Add(i);
        }

        private void OnDetails(Client client)
        {
            if (client == null)
                return;

            // Получаем страницу через DI
            var editPage = App.ServiceProvider.GetService<ClientsEditPage>();
            if (editPage != null)
                editPage.SetClient(client);

            // Навигация через Frame
            var frame = Application.Current.MainWindow.FindName("MainFrame") as Frame;
            if (frame != null && editPage != null)
                frame.Navigate(editPage);
        }

        private bool CanShowDetails(object obj) => obj != null;

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

            var deleted = await _clientBL.DeleteAsync(client.Id);
            if (deleted)
            {
                // Если на странице остался один элемент и это не первая страница — вернемся назад
                if (Clients.Count == 1 && CurrentPage > 1)
                {
                    CurrentPage--;
                }
                else
                {
                    Task.Run(LoadClientsAsync);
                }
            }
        }

        private bool CanDelete(object obj) => obj != null;

        private void OnAddNewClient()
        {
            // Открываем страницу без клиента (создание нового)
            var editPage = App.ServiceProvider.GetService<ClientsEditPage>();
            if (editPage != null)
                editPage.SetClient(null); // или передай новый Client() если требуется

            var frame = Application.Current.MainWindow.FindName("MainFrame") as Frame;
            if (frame != null && editPage != null)
                frame.Navigate(editPage);
        }

        private void OnClearSearch()
        {
            SearchText = string.Empty;
        }

        private void OnPreviousPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }

        private bool CanGoToPreviousPage()
        {
            return CurrentPage > 1;
        }

        private void OnNextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }

        private bool CanGoToNextPage()
        {
            return CurrentPage < TotalPages;
        }

        private void OnGoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
                CurrentPage = page;
        }

        private void OnGoToFirstPage()
        {
            CurrentPage = 1;
        }

        private void OnGoToLastPage()
        {
            CurrentPage = TotalPages;
        }
    
    
    }
}