namespace Entities;

/// <summary>
/// Бизнес-модель отдельного сообщения в чате
/// </summary>
public class Message
{
    /// <summary>Идентификатор сообщения</summary>
    public long Id { get; set; }

    /// <summary>Идентификатор чата, к которому относится сообщение</summary>
    public long ChatId { get; set; }

    /// <summary>Идентификатор отправителя (пользователь или бот)</summary>
    public long SenderUserId { get; set; }

    /// <summary>Текст сообщения</summary>
    public string Text { get; set; }

    /// <summary>Время отправки сообщения</summary>
    public DateTime SentAt { get; set; }
}
