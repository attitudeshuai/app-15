using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.Models;

public enum ReportPeriodType
{
    Monthly,
    Yearly
}

public class ReportCropItem
{
    public Guid CropId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ContainerType { get; set; } = string.Empty;
    public CropStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int GrowthDays { get; set; }
}

public class ReportHarvestStats
{
    public int TotalHarvestRecords { get; set; }
    public decimal TotalHarvestQuantity { get; set; }
    public List<ReportHarvestByCrop> ByCrop { get; set; } = new();
    public List<ReportHarvestByQuality> ByQuality { get; set; } = new();
}

public class ReportHarvestByCrop
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public int HarvestCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal AverageQualityScore { get; set; }
}

public class ReportHarvestByQuality
{
    public HarvestQuality Quality { get; set; }
    public string QualityName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class ReportTaskStats
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CancelledTasks { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal OnTimeRate { get; set; }
    public List<ReportTaskByType> ByType { get; set; } = new();
    public List<ReportTaskByCrop> ByCrop { get; set; } = new();
}

public class ReportTaskByType
{
    public TaskType TaskType { get; set; }
    public string TaskTypeName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Completed { get; set; }
    public decimal CompletionRate { get; set; }
}

public class ReportTaskByCrop
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal CompletionRate { get; set; }
}

public class ReportPestItem
{
    public Guid PestRecordId { get; set; }
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public PestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int DurationDays { get; set; }
}

public class ReportPestStats
{
    public int TotalPestRecords { get; set; }
    public int ActivePestRecords { get; set; }
    public int ResolvedPestRecords { get; set; }
    public decimal ResolutionRate { get; set; }
    public List<ReportPestByType> ByType { get; set; } = new();
    public List<ReportPestItem> Records { get; set; } = new();
}

public class ReportPestByType
{
    public string IssueType { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ResolvedCount { get; set; }
    public decimal ResolutionRate { get; set; }
}

public class PlantingReport
{
    public string ReportTitle { get; set; } = string.Empty;
    public ReportPeriodType PeriodType { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime GeneratedAt { get; set; }

    public int TotalCrops { get; set; }
    public int NewCropsInPeriod { get; set; }
    public List<ReportCropItem> Crops { get; set; } = new();

    public ReportHarvestStats HarvestStats { get; set; } = new();
    public ReportTaskStats TaskStats { get; set; } = new();
    public ReportPestStats PestStats { get; set; } = new();

    public List<string> SummaryHighlights { get; set; } = new();
}
