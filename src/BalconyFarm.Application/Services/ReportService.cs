using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PlantingReport>> GetMonthlyReportAsync(Guid userId, int year, int month, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("生成月度报告: 用户 {UserId}, 年份 {Year}, 月份 {Month}", userId, year, month);

        if (month < 1 || month > 12)
        {
            return ApiResponse<PlantingReport>.Error("月份必须在1-12之间", 400);
        }

        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1).AddTicks(-1);

        var report = await BuildReportAsync(userId, periodStart, periodEnd, ReportPeriodType.Monthly, year, month, plantingLocationId, cancellationToken);
        report.ReportTitle = $"{year}年{month}月种植总结报告";

        return ApiResponse<PlantingReport>.Success(report);
    }

    public async Task<ApiResponse<PlantingReport>> GetYearlyReportAsync(Guid userId, int year, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("生成年度报告: 用户 {UserId}, 年份 {Year}", userId, year);

        var periodStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddYears(1).AddTicks(-1);

        var report = await BuildReportAsync(userId, periodStart, periodEnd, ReportPeriodType.Yearly, year, null, plantingLocationId, cancellationToken);
        report.ReportTitle = $"{year}年度种植总结报告";

        return ApiResponse<PlantingReport>.Success(report);
    }

    private async Task<PlantingReport> BuildReportAsync(
        Guid userId,
        DateTime periodStart,
        DateTime periodEnd,
        ReportPeriodType periodType,
        int year,
        int? month,
        Guid? plantingLocationId,
        CancellationToken cancellationToken)
    {
        var allCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();

        if (plantingLocationId.HasValue)
        {
            allCrops = allCrops.Where(c => c.PlantingLocationId == plantingLocationId.Value).ToList();
        }

        var cropIds = allCrops.Select(c => c.Id).ToList();

        var cropsInPeriod = allCrops
            .Where(c => c.CreatedAt <= periodEnd && (c.Status != CropStatus.Finished || c.CreatedAt >= periodStart))
            .ToList();

        var newCropsInPeriod = allCrops.Count(c => c.CreatedAt >= periodStart && c.CreatedAt <= periodEnd);

        var tasks = (await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken))
            .Where(t => cropIds.Contains(t.CropId) && t.ScheduledDate >= periodStart && t.ScheduledDate <= periodEnd)
            .ToList();

        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => cropIds.Contains(h.CropId) && h.HarvestDate >= periodStart && h.HarvestDate <= periodEnd)
            .ToList();

        var pestRecords = (await _unitOfWork.PestRecords.GetAllAsync(cancellationToken))
            .Where(p => cropIds.Contains(p.CropId) && p.DetectedDate >= periodStart && p.DetectedDate <= periodEnd)
            .ToList();

        var report = new PlantingReport
        {
            PeriodType = periodType,
            Year = year,
            Month = month,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            GeneratedAt = DateTime.UtcNow,
            TotalCrops = cropsInPeriod.Count,
            NewCropsInPeriod = newCropsInPeriod,
            Crops = BuildCropList(cropsInPeriod, periodEnd),
            HarvestStats = BuildHarvestStats(harvestRecords, cropsInPeriod),
            TaskStats = BuildTaskStats(tasks, cropsInPeriod, periodEnd),
            PestStats = BuildPestStats(pestRecords, cropsInPeriod, periodEnd)
        };

        report.SummaryHighlights = GenerateSummaryHighlights(report);

        return report;
    }

    private static List<ReportCropItem> BuildCropList(List<Crop> crops, DateTime periodEnd)
    {
        return crops.Select(c => new ReportCropItem
        {
            CropId = c.Id,
            Name = c.Name,
            Variety = c.Variety,
            PlantingDate = c.PlantingDate,
            Location = c.Location,
            ContainerType = c.ContainerType,
            Status = c.Status,
            StatusName = GetCropStatusName(c.Status),
            GrowthDays = Math.Max(0, (int)(periodEnd.Date - c.PlantingDate.Date).TotalDays)
        }).OrderBy(c => c.PlantingDate).ToList();
    }

    private static ReportHarvestStats BuildHarvestStats(List<HarvestRecord> harvestRecords, List<Crop> crops)
    {
        var cropDict = crops.ToDictionary(c => c.Id);

        var byCrop = harvestRecords
            .GroupBy(h => h.CropId)
            .Select(g =>
            {
                var crop = cropDict.GetValueOrDefault(g.Key);
                var records = g.ToList();
                return new ReportHarvestByCrop
                {
                    CropId = g.Key,
                    CropName = crop?.Name ?? "未知作物",
                    HarvestCount = records.Count,
                    TotalQuantity = Math.Round(records.Sum(h => h.Quantity), 2),
                    Unit = records.FirstOrDefault()?.Unit ?? string.Empty,
                    AverageQualityScore = Math.Round((decimal)records.Average(h => (int)h.Quality), 2)
                };
            })
            .OrderByDescending(h => h.TotalQuantity)
            .ToList();

        var totalRecords = harvestRecords.Count;
        var byQuality = Enum.GetValues(typeof(HarvestQuality)).Cast<HarvestQuality>()
            .Select(q =>
            {
                var count = harvestRecords.Count(h => h.Quality == q);
                return new ReportHarvestByQuality
                {
                    Quality = q,
                    QualityName = GetQualityName(q),
                    Count = count,
                    Percentage = totalRecords > 0 ? Math.Round((decimal)count / totalRecords * 100, 1) : 0,
                    TotalQuantity = Math.Round(harvestRecords.Where(h => h.Quality == q).Sum(h => h.Quantity), 2)
                };
            })
            .ToList();

        return new ReportHarvestStats
        {
            TotalHarvestRecords = totalRecords,
            TotalHarvestQuantity = Math.Round(harvestRecords.Sum(h => h.Quantity), 2),
            ByCrop = byCrop,
            ByQuality = byQuality
        };
    }

    private static ReportTaskStats BuildTaskStats(List<CropCareTask> tasks, List<Crop> crops, DateTime periodEnd)
    {
        var cropDict = crops.ToDictionary(c => c.Id);
        var effectiveTasks = tasks.Where(t => t.Status != TaskStatus.Cancelled).ToList();
        var totalEffective = effectiveTasks.Count;
        var completedTasks = effectiveTasks.Count(t => t.Status == TaskStatus.Completed);
        var onTimeCompleted = effectiveTasks.Count(t =>
            t.Status == TaskStatus.Completed &&
            t.CompletedDate.HasValue &&
            t.CompletedDate.Value.Date <= t.ScheduledDate.Date);
        var overdueTasks = effectiveTasks.Count(t =>
            (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress) &&
            t.ScheduledDate.Date < periodEnd.Date);

        var byType = Enum.GetValues(typeof(TaskType)).Cast<TaskType>()
            .Select(tt =>
            {
                var typeTasks = tasks.Where(t => t.TaskType == tt).ToList();
                var typeEffective = typeTasks.Where(t => t.Status != TaskStatus.Cancelled).ToList();
                var typeCompleted = typeEffective.Count(t => t.Status == TaskStatus.Completed);
                return new ReportTaskByType
                {
                    TaskType = tt,
                    TaskTypeName = GetTaskTypeName(tt),
                    Total = typeEffective.Count,
                    Completed = typeCompleted,
                    CompletionRate = typeEffective.Count > 0 ? Math.Round((decimal)typeCompleted / typeEffective.Count * 100, 1) : 0
                };
            })
            .Where(t => t.Total > 0)
            .OrderByDescending(t => t.Total)
            .ToList();

        var byCrop = tasks
            .GroupBy(t => t.CropId)
            .Select(g =>
            {
                var crop = cropDict.GetValueOrDefault(g.Key);
                var cropTasks = g.ToList();
                var cropEffective = cropTasks.Where(t => t.Status != TaskStatus.Cancelled).ToList();
                var cropCompleted = cropEffective.Count(t => t.Status == TaskStatus.Completed);
                return new ReportTaskByCrop
                {
                    CropId = g.Key,
                    CropName = crop?.Name ?? "未知作物",
                    TotalTasks = cropEffective.Count,
                    CompletedTasks = cropCompleted,
                    CompletionRate = cropEffective.Count > 0 ? Math.Round((decimal)cropCompleted / cropEffective.Count * 100, 1) : 0
                };
            })
            .OrderByDescending(c => c.TotalTasks)
            .ToList();

        return new ReportTaskStats
        {
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks,
            PendingTasks = effectiveTasks.Count(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress),
            OverdueTasks = overdueTasks,
            CancelledTasks = tasks.Count(t => t.Status == TaskStatus.Cancelled),
            CompletionRate = totalEffective > 0 ? Math.Round((decimal)completedTasks / totalEffective * 100, 1) : 0,
            OnTimeRate = completedTasks > 0 ? Math.Round((decimal)onTimeCompleted / completedTasks * 100, 1) : 0,
            ByType = byType,
            ByCrop = byCrop
        };
    }

    private static ReportPestStats BuildPestStats(List<PestRecord> pestRecords, List<Crop> crops, DateTime periodEnd)
    {
        var cropDict = crops.ToDictionary(c => c.Id);

        var records = pestRecords.Select(p =>
        {
            var crop = cropDict.GetValueOrDefault(p.CropId);
            var endDate = p.ResolvedDate ?? periodEnd;
            var durationDays = Math.Max(0, (int)(endDate.Date - p.DetectedDate.Date).TotalDays);
            return new ReportPestItem
            {
                PestRecordId = p.Id,
                CropId = p.CropId,
                CropName = crop?.Name ?? "未知作物",
                IssueType = p.IssueType,
                Symptoms = p.Symptoms,
                Treatment = p.Treatment,
                DetectedDate = p.DetectedDate,
                ResolvedDate = p.ResolvedDate,
                Status = p.Status,
                StatusName = GetPestStatusName(p.Status),
                DurationDays = durationDays
            };
        })
        .OrderBy(p => p.DetectedDate)
        .ToList();

        var byType = pestRecords
            .GroupBy(p => p.IssueType)
            .Select(g =>
            {
                var typeRecords = g.ToList();
                var resolvedCount = typeRecords.Count(p => p.Status == PestStatus.Resolved);
                return new ReportPestByType
                {
                    IssueType = g.Key,
                    Count = typeRecords.Count,
                    ResolvedCount = resolvedCount,
                    ResolutionRate = typeRecords.Count > 0 ? Math.Round((decimal)resolvedCount / typeRecords.Count * 100, 1) : 0
                };
            })
            .OrderByDescending(t => t.Count)
            .ToList();

        var total = pestRecords.Count;
        var resolved = pestRecords.Count(p => p.Status == PestStatus.Resolved);

        return new ReportPestStats
        {
            TotalPestRecords = total,
            ActivePestRecords = pestRecords.Count(p => p.Status != PestStatus.Resolved),
            ResolvedPestRecords = resolved,
            ResolutionRate = total > 0 ? Math.Round((decimal)resolved / total * 100, 1) : 0,
            ByType = byType,
            Records = records
        };
    }

    private static List<string> GenerateSummaryHighlights(PlantingReport report)
    {
        var highlights = new List<string>();

        if (report.NewCropsInPeriod > 0)
        {
            highlights.Add($"本期新增种植作物 {report.NewCropsInPeriod} 种");
        }

        if (report.HarvestStats.TotalHarvestRecords > 0)
        {
            highlights.Add($"累计收获 {report.HarvestStats.TotalHarvestRecords} 次，总产量 {report.HarvestStats.TotalHarvestQuantity} 单位");

            var topCrop = report.HarvestStats.ByCrop.FirstOrDefault();
            if (topCrop != null)
            {
                highlights.Add($"产量冠军：「{topCrop.CropName}」总产量 {topCrop.TotalQuantity} 单位");
            }
        }

        if (report.TaskStats.TotalTasks > 0)
        {
            highlights.Add($"养护任务完成率 {report.TaskStats.CompletionRate}%，按时完成率 {report.TaskStats.OnTimeRate}%");
        }

        if (report.PestStats.TotalPestRecords > 0)
        {
            highlights.Add($"病虫害记录 {report.PestStats.TotalPestRecords} 条，解决率 {report.PestStats.ResolutionRate}%");
        }

        if (!highlights.Any())
        {
            highlights.Add("本期暂无种植活动记录");
        }

        return highlights;
    }

    private static string GetCropStatusName(CropStatus status)
    {
        return status switch
        {
            CropStatus.Growing => "生长中",
            CropStatus.Harvesting => "可收获",
            CropStatus.Finished => "已结束",
            _ => status.ToString()
        };
    }

    private static string GetTaskTypeName(TaskType type)
    {
        return type switch
        {
            TaskType.Water => "浇水",
            TaskType.Fertilize => "施肥",
            TaskType.Prune => "修剪",
            TaskType.Repot => "换盆",
            _ => type.ToString()
        };
    }

    private static string GetPestStatusName(PestStatus status)
    {
        return status switch
        {
            PestStatus.Detected => "已发现",
            PestStatus.Treating => "治疗中",
            PestStatus.Resolved => "已解决",
            _ => status.ToString()
        };
    }

    private static string GetQualityName(HarvestQuality quality)
    {
        return quality switch
        {
            HarvestQuality.Poor => "较差",
            HarvestQuality.Fair => "一般",
            HarvestQuality.Good => "良好",
            HarvestQuality.Excellent => "优秀",
            _ => quality.ToString()
        };
    }
}
