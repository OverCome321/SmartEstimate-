namespace Entities;

/// <summary>
/// Бизнес-модель чата (диалога)
/// </summary>
public class Chat
{
    /// <summary>Идентификатор чата</summary>
    public long Id { get; set; }

    /// <summary>Идентификатор пользователя-владельца чата</summary>
    public long UserId { get; set; }

    /// <summary>Время создания (начала) чата</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Сообщения в рамках диалога</summary>
    public IList<Message> Messages { get; set; } = new List<Message>();
}
