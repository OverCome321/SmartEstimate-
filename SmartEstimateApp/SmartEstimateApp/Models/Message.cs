namespace SmartEstimateApp.Models;

/// <summary>
/// UI-модель отдельного сообщения в чате
/// </summary>
public class Message
{
    /// <summary>Идентификатор сообщения</summary>
    public long Id { get; set; }

    /// <summary>Идентификатор чата</summary>
    public long ChatId { get; set; }

    /// <summary>Идентификатор отправителя</summary>
    public long SenderUserId { get; set; }

    /// <summary>Текст сообщения</summary>
    public string Text { get; set; } = "";

    /// <summary>Время отправки</summary>
    public DateTime SentAt { get; set; }
}
