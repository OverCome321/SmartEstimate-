namespace SmartEstimateApp.Models;

public class Project
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int Status { get; set; }

    public long ClientId { get; set; }

    public string ClientName { get; set; }

    public long UserId { get; set; }

}