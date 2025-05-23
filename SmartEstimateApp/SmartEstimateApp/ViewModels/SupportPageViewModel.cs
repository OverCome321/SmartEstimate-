using OpenAIService.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels;

public class SupportPageViewModel : PropertyChangedBase
{
    private readonly IOpenAiService _openAiService;
    private string _currentMessage = string.Empty;
    private bool _isLoading;

    public ObservableCollection<ChatMessage> Messages { get; }

    public string CurrentMessage
    {
        get => _currentMessage;
        set
        {
            if (SetProperty(ref _currentMessage, value, nameof(CurrentMessage)))
            {
                OnPropertyChanged(nameof(CanSendMessage));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value, nameof(IsLoading));
    }

    public bool CanSendMessage => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsLoading;

    public ICommand SendMessageCommand { get; }

    public SupportPageViewModel(IOpenAiService openAiService)
    {
        _openAiService = openAiService;

        Messages = new ObservableCollection<ChatMessage>();
        SendMessageCommand = new RelayCommand(
            async _ => await SendMessageAsync(),
            _ => CanSendMessage
        );

        Messages.Add(new ChatMessage
        {
            Content = "Здравствуйте! Я ваш AI-ассистент. Чем могу помочь?",
            IsFromUser = false,
            Timestamp = DateTime.Now
        });
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage) || IsLoading)
            return;

        var userMessage = CurrentMessage.Trim();
        CurrentMessage = string.Empty;

        Messages.Add(new ChatMessage
        {
            Content = userMessage,
            IsFromUser = true,
            Timestamp = DateTime.Now
        });

        IsLoading = true;

        try
        {
            var response = await _openAiService.AskAsync(userMessage);

            Messages.Add(new ChatMessage
            {
                Content = response,
                IsFromUser = false,
                Timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            Messages.Add(new ChatMessage
            {
                Content = $"Извините, произошла ошибка: {ex.Message}",
                IsFromUser = false,
                Timestamp = DateTime.Now,
                IsError = true
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}