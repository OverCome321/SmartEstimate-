using System.ComponentModel.DataAnnotations;

namespace Dal.DbModels;

public class Message
{
    public long Id { get; set; }

    public long ChatId { get; set; }
    public Chat Chat { get; set; }

    public long SenderUserId { get; set; }
    public User SenderUser { get; set; }

    [Required]
    public string Text { get; set; }

    public DateTime SentAt { get; set; }
}