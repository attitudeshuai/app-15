namespace BalconyFarm.Application.Models;

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
