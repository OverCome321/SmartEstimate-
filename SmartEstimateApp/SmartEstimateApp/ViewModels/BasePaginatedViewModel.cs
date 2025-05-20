using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels;

public abstract class BasePaginatedViewModel<T> : PropertyChangedBase
{
    // Главная коллекция для отображения (заполняется в наследнике)
    public ObservableCollection<T> Items { get; set; } = new();

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
                Task.Run(LoadItemsAsync);
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
                Task.Run(LoadItemsAsync);
            }
        }
    }
    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value, nameof(SearchText)))
            {
                CurrentPage = 1;
                Task.Run(LoadItemsAsync);
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
    private bool _isCardViewSelected = true;
    public bool IsCardViewSelected
    {
        get => _isCardViewSelected;
        set
        {
            if (SetProperty(ref _isCardViewSelected, value, nameof(IsCardViewSelected)))
            {
                _isCardViewSelected = value;
                if (value && _isTableViewSelected)
                    IsTableViewSelected = false;
            }
        }
    }
    private bool _isTableViewSelected = false;
    public bool IsTableViewSelected
    {
        get => _isTableViewSelected;
        set
        {
            if (SetProperty(ref _isTableViewSelected, value, nameof(IsTableViewSelected)))
            {
                _isTableViewSelected = value;
                if (value && _isCardViewSelected)
                    IsCardViewSelected = false;
            }
        }
    }

    public bool HasResults => Items.Any();
    public string ResultsInfo => $"Найдено {TotalCount}";
    public string PageInfo => $"Страница {CurrentPage} из {TotalPages}";

    public List<int> PageSizeOptions { get; } = new List<int> { 10, 20, 50, 100 };

    private ObservableCollection<int> _pageNumbers = new();
    public ObservableCollection<int> PageNumbers
    {
        get => _pageNumbers;
        set => SetProperty(ref _pageNumbers, value, nameof(PageNumbers));
    }
    private bool _isTableView = false;
    public bool IsTableView
    {
        get => _isTableView;
        set => SetProperty(ref _isTableView, value, nameof(IsTableView));
    }

    // Абстрактный метод для загрузки данных (реализуется в наследниках)
    protected abstract Task LoadItemsAsync();

    // Pagination commands
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand GoToPageCommand { get; }
    public ICommand GoToFirstPageCommand { get; }
    public ICommand GoToLastPageCommand { get; }
    public ICommand SwitchToTableViewCommand { get; }
    public ICommand SwitchToCardViewCommand { get; }
    public ICommand SetViewModeCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public BasePaginatedViewModel()
    {
        SetViewModeCommand = new RelayCommand(obj => SetViewMode(obj as string));
        PreviousPageCommand = new RelayCommand(_ => OnPreviousPage(), _ => CanGoToPreviousPage());
        NextPageCommand = new RelayCommand(_ => OnNextPage(), _ => CanGoToNextPage());
        GoToPageCommand = new RelayCommand(obj => OnGoToPage(Convert.ToInt32(obj)));
        GoToFirstPageCommand = new RelayCommand(_ => OnGoToFirstPage(), _ => CanGoToPreviousPage());
        GoToLastPageCommand = new RelayCommand(_ => OnGoToLastPage(), _ => CanGoToNextPage());
        SwitchToTableViewCommand = new RelayCommand(_ => IsTableView = true);
        SwitchToCardViewCommand = new RelayCommand(_ => IsTableView = false);
        ClearSearchCommand = new RelayCommand(_ => OnClearSearch());
    }

    protected void SetViewMode(string viewMode)
    {
        switch (viewMode)
        {
            case "Card":
                IsCardViewSelected = true;
                IsTableViewSelected = false;
                break;
            case "Table":
                IsCardViewSelected = false;
                IsTableViewSelected = true;
                break;
        }
    }

    protected void UpdatePageNumbers()
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

    protected void OnPreviousPage()
    {
        if (CurrentPage > 1)
            CurrentPage--;
    }

    protected bool CanGoToPreviousPage() => CurrentPage > 1;

    protected void OnNextPage()
    {
        if (CurrentPage < TotalPages)
            CurrentPage++;
    }

    protected bool CanGoToNextPage() => CurrentPage < TotalPages;

    protected void OnGoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
            CurrentPage = page;
    }

    protected void OnGoToFirstPage()
    {
        CurrentPage = 1;
    }

    protected void OnGoToLastPage()
    {
        CurrentPage = TotalPages;
    }
    protected void OnClearSearch()
    {
        SearchText = string.Empty;
    }
}
