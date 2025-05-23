namespace SmartEstimateApp.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsError { get; set; }
}