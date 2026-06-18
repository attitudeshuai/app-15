using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.Data;

public class CareTaskRule
{
    public TaskType TaskType { get; set; }
    public int IntervalDays { get; set; }
    public string? DefaultNote { get; set; }
}

public class GrowthStageCareRule
{
    public GrowthStage Stage { get; set; }
    public int StartDay { get; set; }
    public int EndDay { get; set; }
    public List<CareTaskRule> Tasks { get; set; } = new();
}

public class CropCareRule
{
    public string CropName { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = new();
    public int TotalGrowthDays { get; set; }
    public List<GrowthStageCareRule> StageRules { get; set; } = new();
}
