using AutoMapper;
using Bl.Interfaces;
using Common.Search;
using OpenAIService.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Mapper = SmartEstimateApp.Mappings.Mapper;

namespace SmartEstimateApp.ViewModels;

public class SupportPageViewModel : PropertyChangedBase
{
    private const long BotUserId = 1;
    private const string WelcomeText =
        "Здравствуйте! Я ваш AI-ассистент SmartEstimate. Чем могу помочь вам сегодня?";

    private readonly IChatBL _chatBL;
    private readonly IOpenAiService _openAiService;
    private readonly HomeWindowViewModel _homeWindowViewModel;
    private readonly long _currentUserId;

    private Chat? _selectedChat;
    private string _currentMessage = string.Empty;
    private bool _isLoading;

    public ObservableCollection<Chat> Chats { get; } = new ObservableCollection<Chat>();
    public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

    public Chat? SelectedChat
    {
        get => _selectedChat;
        set
        {
            if (SetProperty(ref _selectedChat, value, nameof(SelectedChat)))
                _ = OnChatSelectedAsync();
        }
    }

    public string CurrentMessage
    {
        get => _currentMessage;
        set
        {
            if (SetProperty(ref _currentMessage, value, nameof(CurrentMessage)))
                OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value, nameof(IsLoading)))
                OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public bool CanSendMessage =>
        SelectedChat != null
        && !string.IsNullOrWhiteSpace(CurrentMessage)
        && !IsLoading;

    public ICommand CreateNewChatCommand { get; }
    public ICommand SendMessageCommand { get; }
    public ICommand SelectChatCommand { get; }
    public ICommand DeleteChatCommand { get; }

    public SupportPageViewModel(
        IChatBL chatBL,
        IOpenAiService openAiService,
        IMapper mapper,
        HomeWindowViewModel homeWindowViewModel)
    {
        _chatBL = chatBL ?? throw new ArgumentNullException(nameof(chatBL));
        _openAiService = openAiService ?? throw new ArgumentNullException(nameof(openAiService));
        _homeWindowViewModel = homeWindowViewModel ?? throw new ArgumentNullException(nameof(homeWindowViewModel));

        _currentUserId = CurrentUser.Instance.User?.Id
            ?? throw new InvalidOperationException("Пользователь не аутентифицирован");

        CreateNewChatCommand = new RelayCommand(async _ => await CreateNewChatAsync(), _ => !IsLoading);
        SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage);
        SelectChatCommand = new RelayCommand(OnSelectChat);
        DeleteChatCommand = new RelayCommand(async param => await OnDeleteChatAsync(param));

        _ = LoadChatsAsync();
    }

    private void OnSelectChat(object parameter)
    {
        if (parameter is Chat chat)
            SelectedChat = chat;
    }

    private async Task OnDeleteChatAsync(object parameter)
    {
        if (!(parameter is Chat chat)) return;

        var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить чат «{chat.Title}»?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _chatBL.DeleteAsync(chat.Id);
            await LoadChatsAsync();
        }
        catch
        {
            _homeWindowViewModel.ShowError("Ошибка при удалении чата");
        }
    }

    private async Task LoadChatsAsync()
    {
        try
        {
            IsLoading = true;
            Chats.Clear();

            var search = new ChatSearchParams { UserId = _currentUserId };
            var result = await _chatBL.GetAsync(search, includeMessages: false);

            var uiChats = result.Objects
                .Select(c => Mapper.ToModel(c))
                .OrderBy(c => c.StartedAt)
                .ToList();

            foreach (var chat in uiChats)
                Chats.Add(chat);

            SelectedChat = Chats.LastOrDefault();
        }
        catch
        {
            _homeWindowViewModel.ShowError(
                "Непредвиденная ошибка при загрузке чатов, попробуйте снова");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateNewChatAsync()
    {
        try
        {
            IsLoading = true;

            // 1) создаём новый чат
            var newChatId = await _chatBL.CreateChatAsync(_currentUserId);
            // 2) сразу отправляем туда приветственное сообщение
            await _chatBL.SendMessageAsync(newChatId, BotUserId, WelcomeText);

            // 3) обновляем список
            await LoadChatsAsync();

            // 4) и теперь — явно выбираем именно созданный:
            SelectedChat = Chats.FirstOrDefault(c => c.Id == newChatId);
        }
        catch
        {
            _homeWindowViewModel.ShowError(
                "Непредвиденная ошибка при создании нового чата, попробуйте снова");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }


    private async Task OnChatSelectedAsync()
    {
        Messages.Clear();
        if (SelectedChat == null)
            return;

        try
        {
            IsLoading = true;

            var fullChat = await _chatBL.GetAsync(
                SelectedChat.Id, includeRelated: true);

            if (fullChat != null)
            {
                foreach (var msg in fullChat.Messages)
                    Messages.Add(Mapper.ToModel(msg));
            }
        }
        catch
        {
            _homeWindowViewModel.ShowError(
                "Непредвиденная ошибка при загрузке сообщений, попробуйте снова");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    private async Task SendMessageAsync()
    {
        if (!CanSendMessage || SelectedChat == null)
            return;

        var text = CurrentMessage.Trim();
        CurrentMessage = string.Empty;

        // добавить сообщение пользователя в UI
        Messages.Add(new Message
        {
            ChatId = SelectedChat.Id,
            SenderUserId = _currentUserId,
            Text = text,
            SentAt = DateTime.Now
        });

        try
        {
            IsLoading = true;

            // сохранить пользователя
            await _chatBL.SendMessageAsync(SelectedChat.Id, _currentUserId, text);

            // получить ответ AI
            var aiResponse = await _openAiService.AskAsync(text);

            // отобразить в UI
            var botMsg = new Message
            {
                ChatId = SelectedChat.Id,
                SenderUserId = BotUserId,
                Text = aiResponse,
                SentAt = DateTime.Now
            };
            Messages.Add(botMsg);

            // сохранить AI-ответ в БД
            await _chatBL.SendMessageAsync(SelectedChat.Id, BotUserId, aiResponse);
        }
        catch (Exception ex)
        {
            Messages.Add(new Message
            {
                ChatId = SelectedChat.Id,
                Text = $"Ошибка: {ex.Message}",
                SentAt = DateTime.Now
            });
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }
}
