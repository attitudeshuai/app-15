namespace BalconyFarm.Domain.Entities;

public class CommunityQuestion
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public int ViewCount { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<CommunityReply> Replies { get; set; } = new List<CommunityReply>();
    public ICollection<CommunityTag> Tags { get; set; } = new List<CommunityTag>();
}
