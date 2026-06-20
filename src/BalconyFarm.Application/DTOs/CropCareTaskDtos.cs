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

    public bool WeatherAdjusted { get; set; }
    public string? WeatherAdjustmentReason { get; set; }
    public DateTime? WeatherAdjustedAt { get; set; }
    public string? WeatherCity { get; set; }
    public double? WeatherTemperatureC { get; set; }
    public double? WeatherPrecipitationMm { get; set; }
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
    public bool EnableWeatherAware { get; set; } = true;
}

public class RecommendedTaskDto
{
    public TaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string? Note { get; set; }
    public GrowthStage GrowthStage { get; set; }
    public string GrowthStageName { get; set; } = string.Empty;

    public bool WeatherAdjusted { get; set; }
    public string? WeatherAdjustmentReason { get; set; }
    public DailyWeatherDto? WeatherInfo { get; set; }
    public bool WeatherSkipped { get; set; }
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
    public int WeatherSkippedCount { get; set; }
    public int WeatherAdjustedCount { get; set; }
    public string? WeatherCity { get; set; }
    public List<RecommendedTaskDto> RecommendedTasks { get; set; } = new();
    public List<CropCareTaskDto> CreatedTasks { get; set; } = new();
}

public class BatchUpdateTaskStatusRequestDto
{
    public List<Guid> TaskIds { get; set; } = new();
    public Domain.Enums.TaskStatus Status { get; set; }
}

public class BatchUpdateTaskStatusResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<CropCareTaskDto> UpdatedTasks { get; set; } = new();
    public List<BatchTaskFailureDto> Failures { get; set; } = new();
}

public class BatchTaskFailureDto
{
    public Guid TaskId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class DailyWeatherDto
{
    public DateTime Date { get; set; }
    public double TemperatureC { get; set; }
    public double PrecipitationMm { get; set; }
    public double Humidity { get; set; }
    public string WeatherCondition { get; set; } = string.Empty;
    public bool IsRaining => PrecipitationMm > 0;
    public bool IsHeavyRain => PrecipitationMm >= 20;
}

public class WeatherForecastDto
{
    public string CityName { get; set; } = string.Empty;
    public List<DailyWeatherDto> DailyForecasts { get; set; } = new();
}

public class WeatherAdjustTaskRequestDto
{
    public Guid? CropId { get; set; }
    public int DaysAhead { get; set; } = 7;
    public bool DryRun { get; set; } = true;
}

public class WeatherAdjustTaskResultDto
{
    public int TotalTasksChecked { get; set; }
    public int TasksSkipped { get; set; }
    public int TasksDelayed { get; set; }
    public int TasksUnchanged { get; set; }
    public int TasksFailed { get; set; }
    public List<WeatherAdjustedTaskDto> AdjustedTasks { get; set; } = new();
    public List<WeatherAdjustTaskFailureDto> Failures { get; set; } = new();
}

public class WeatherAdjustedTaskDto
{
    public Guid TaskId { get; set; }
    public Guid CropId { get; set; }
    public string? CropName { get; set; }
    public DateTime OriginalScheduledDate { get; set; }
    public DateTime? NewScheduledDate { get; set; }
    public string Action { get; set; } = string.Empty;
    public string AdjustmentReason { get; set; } = string.Empty;
    public DailyWeatherDto? WeatherOnScheduleDate { get; set; }
}

public class WeatherAdjustTaskFailureDto
{
    public Guid TaskId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
