using BalconyFarm.Domain.Enums;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class PlantingPlanTemplateDto
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
    public List<PlantingPlanTemplateStageDto> Stages { get; set; } = new();
    public int TotalTasks { get; set; }
}

public class PlantingPlanTemplateStageDto
{
    public GrowthStage Stage { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public int EndDay { get; set; }
    public string? Description { get; set; }
    public List<PlantingPlanTemplateTaskDto> Tasks { get; set; } = new();
}

public class PlantingPlanTemplateTaskDto
{
    public TaskType TaskType { get; set; }
    public int DaysAfterPlanting { get; set; }
    public string? DefaultNote { get; set; }
    public GrowthStage? GrowthStage { get; set; }
}

public class ApplyTemplateRequestDto
{
    public string TemplateId { get; set; } = string.Empty;
    public Guid CropId { get; set; }
    public DateTime PlantingDate { get; set; }
    public bool OverwriteExisting { get; set; } = false;
}

public class ApplyTemplateResultDto
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int CreatedTaskCount { get; set; }
    public int SkippedTaskCount { get; set; }
    public List<CropCareTaskDto> CreatedTasks { get; set; } = new();
}

public class PlantingPlanTemplateQueryRequestDto : PagedRequest
{
    public string? Keyword { get; set; }
    public string? Difficulty { get; set; }
}
