using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.DTOs;

public class AchievementDto
{
    public Guid Id { get; set; }
    public AchievementType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public string? UnlockReason { get; set; }
}

public class UserAchievementDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }
    public string? UnlockReason { get; set; }
    public AchievementDto? Achievement { get; set; }
}

public class AchievementSummaryDto
{
    public int TotalCount { get; set; }
    public int UnlockedCount { get; set; }
    public int TotalPoints { get; set; }
    public int EarnedPoints { get; set; }
    public List<AchievementDto> Achievements { get; set; } = new();
}
