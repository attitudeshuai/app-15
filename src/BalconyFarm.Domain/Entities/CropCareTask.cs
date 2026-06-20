using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Domain.Entities;

public class CropCareTask
{
    public Guid Id { get; set; }
    public Guid CropId { get; set; }
    public TaskType TaskType { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Domain.Enums.TaskStatus Status { get; set; } = Domain.Enums.TaskStatus.Pending;
    public string? Note { get; set; }

    public bool WeatherAdjusted { get; set; }
    public string? WeatherAdjustmentReason { get; set; }
    public DateTime? WeatherAdjustedAt { get; set; }
    public string? WeatherCity { get; set; }
    public double? WeatherTemperatureC { get; set; }
    public double? WeatherPrecipitationMm { get; set; }

    public Crop? Crop { get; set; }
}
