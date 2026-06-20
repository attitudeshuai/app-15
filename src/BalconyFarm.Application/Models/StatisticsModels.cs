using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Application.Models;

public class PeriodComparison<T>
{
    public T CurrentValue { get; set; } = default!;
    public T PreviousValue { get; set; } = default!;
    public T Change { get; set; } = default!;
    public decimal? ChangePercentage { get; set; }
}

public class OverviewComparison
{
    public PeriodComparison<int> NewCrops { get; set; } = new();
    public PeriodComparison<decimal> HarvestQuantity { get; set; } = new();
    public PeriodComparison<decimal> TaskCompletionRate { get; set; } = new();
}

public class OverviewStats
{
    public int TotalCrops { get; set; }
    public int GrowingCrops { get; set; }
    public int HarvestingCrops { get; set; }
    public int FinishedCrops { get; set; }
    public int TotalTasks { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTaskCount { get; set; }
    public int TotalHarvestRecords { get; set; }
    public decimal TotalHarvestQuantity { get; set; }
    public int ActivePestIssues { get; set; }
    public OverviewComparison ComparedToLastMonth { get; set; } = new();
    public OverviewComparison ComparedToLastYear { get; set; } = new();
}

public class TrendData
{
    public string Period { get; set; } = string.Empty;
    public int NewCrops { get; set; }
    public int CompletedTasks { get; set; }
    public int HarvestRecords { get; set; }
    public decimal HarvestQuantity { get; set; }
}

public class CropRankingItem
{
    public string CropName { get; set; } = string.Empty;
    public decimal TotalHarvest { get; set; }
    public int HarvestCount { get; set; }
}

public class CropTaskCompletionItem
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OnTimeCompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal OnTimeRate { get; set; }
}

public class QualityDistributionItem
{
    public HarvestQuality Quality { get; set; }
    public string QualityName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class CropQualityStats
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public int TotalHarvests { get; set; }
    public int HighQualityHarvests { get; set; }
    public decimal HighQualityRate { get; set; }
    public decimal AverageQualityScore { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class LocationQualityStats
{
    public string Location { get; set; } = string.Empty;
    public int TotalHarvests { get; set; }
    public int HighQualityHarvests { get; set; }
    public decimal HighQualityRate { get; set; }
    public decimal AverageQualityScore { get; set; }
    public int CropCount { get; set; }
}

public class SeasonQualityStats
{
    public string Season { get; set; } = string.Empty;
    public int TotalHarvests { get; set; }
    public int HighQualityHarvests { get; set; }
    public decimal HighQualityRate { get; set; }
    public decimal AverageQualityScore { get; set; }
    public decimal TotalQuantity { get; set; }
}

public class ContainerQualityStats
{
    public string ContainerType { get; set; } = string.Empty;
    public int TotalHarvests { get; set; }
    public int HighQualityHarvests { get; set; }
    public decimal HighQualityRate { get; set; }
    public decimal AverageQualityScore { get; set; }
}

public class QualityInsight
{
    public string Dimension { get; set; } = string.Empty;
    public string TopPerformer { get; set; } = string.Empty;
    public decimal TopHighQualityRate { get; set; }
    public string BottomPerformer { get; set; } = string.Empty;
    public decimal BottomHighQualityRate { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class HarvestQualityAnalysis
{
    public List<QualityDistributionItem> QualityDistribution { get; set; } = new();
    public List<CropQualityStats> ByCrop { get; set; } = new();
    public List<LocationQualityStats> ByLocation { get; set; } = new();
    public List<SeasonQualityStats> BySeason { get; set; } = new();
    public List<ContainerQualityStats> ByContainer { get; set; } = new();
    public List<QualityInsight> Insights { get; set; } = new();
    public decimal OverallAverageQualityScore { get; set; }
    public decimal OverallHighQualityRate { get; set; }
    public int TotalHarvestRecords { get; set; }
}
