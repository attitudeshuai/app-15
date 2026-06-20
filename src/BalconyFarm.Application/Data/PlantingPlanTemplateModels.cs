using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.Data;

public class PlantingPlanTemplateTask
{
    public TaskType TaskType { get; set; }
    public int DaysAfterPlanting { get; set; }
    public string? DefaultNote { get; set; }
    public GrowthStage? GrowthStage { get; set; }
}

public class PlantingPlanTemplateStage
{
    public GrowthStage Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public int EndDay { get; set; }
    public string? Description { get; set; }
    public List<PlantingPlanTemplateTask> Tasks { get; set; } = new();
}

public class PlantingPlanTemplate
{
    public string Id { get; set; } = string.Empty;
    public string CropName { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = new();
    public string Difficulty { get; set; } = string.Empty;
    public int TotalGrowthDays { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Tips { get; set; }
    public string? SuitableLocation { get; set; }
    public string? DefaultContainerType { get; set; }
    public List<PlantingPlanTemplateStage> Stages { get; set; } = new();
    public List<PlantingPlanTemplateTask> Tasks { get; set; } = new();
}
