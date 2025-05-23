namespace SmartEstimateApp.Models;

/// <summary>
/// UI-модель чата (диалога)
/// </summary>
public class Chat
{
    /// <summary>Идентификатор чата</summary>
    public long Id { get; set; }

    /// <summary>Id пользователя-владельца</summary>
    public long UserId { get; set; }

    /// <summary>Когда началcя чат</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>История сообщений</summary>
    public IList<Message> Messages { get; set; } = new List<Message>();

    public string Title => $"Чат #{Id}";
}
