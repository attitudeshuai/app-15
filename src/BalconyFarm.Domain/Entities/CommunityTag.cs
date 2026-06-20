namespace BalconyFarm.Domain.Entities;

public class CommunityTag
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CommunityQuestion? Question { get; set; }
}
