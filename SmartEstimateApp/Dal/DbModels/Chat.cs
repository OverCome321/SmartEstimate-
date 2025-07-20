namespace Dal.DbModels;

public class Chat
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }

    public DateTime StartedAt { get; set; }

    public ICollection<Message> Messages { get; set; }
}
