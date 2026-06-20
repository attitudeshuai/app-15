using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }
    public AchievementType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
