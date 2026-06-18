using BalconyFarm.Domain.Enums;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class CropCareTaskDto
{
    public Guid Id { get; set; }
    public Guid CropId { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Domain.Enums.TaskStatus Status { get; set; }
    public string? Note { get; set; }
    public string? CropName { get; set; }
    public bool IsOverdue { get; set; }
    public int? OverdueDays { get; set; }
}

public class CreateCropCareTaskRequestDto
{
    public Guid CropId { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Note { get; set; }
}

public class UpdateCropCareTaskRequestDto
{
    public TaskType? TaskType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public Domain.Enums.TaskStatus? Status { get; set; }
    public string? Note { get; set; }
}

public class UpdateTaskStatusRequestDto
{
    public Domain.Enums.TaskStatus Status { get; set; }
}

public class CropCareTaskQueryRequestDto : PagedRequest
{
    public Guid? CropId { get; set; }
    public TaskType? TaskType { get; set; }
    public Domain.Enums.TaskStatus? Status { get; set; }
    public DateTime? ScheduledDateFrom { get; set; }
    public DateTime? ScheduledDateTo { get; set; }
}

public class GenerateCareTasksRequestDto
{
    public Guid CropId { get; set; }
    public int DaysAhead { get; set; } = 30;
    public bool OverwriteExisting { get; set; } = false;
}

public class RecommendedTaskDto
{
    public TaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Note { get; set; }
    public GrowthStage GrowthStage { get; set; }
    public string GrowthStageName { get; set; } = string.Empty;
}

public class GenerateCareTasksResultDto
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public GrowthStage CurrentGrowthStage { get; set; }
    public string CurrentGrowthStageName { get; set; } = string.Empty;
    public int DaysSincePlanting { get; set; }
    public int TotalGrowthDays { get; set; }
    public int GeneratedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<RecommendedTaskDto> RecommendedTasks { get; set; } = new();
    public List<CropCareTaskDto> CreatedTasks { get; set; } = new();
}
