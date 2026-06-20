using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(IUnitOfWork unitOfWork, ILogger<StatisticsService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<OverviewStats>> GetOverviewStatsAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户总览统计: {UserId}, 位置: {LocationId}", userId, plantingLocationId);

        var crops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();

        if (plantingLocationId.HasValue)
        {
            crops = crops.Where(c => c.PlantingLocationId == plantingLocationId.Value).ToList();
        }
        var cropIds = crops.Select(c => c.Id).ToList();

        var tasks = (await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken))
            .Where(t => cropIds.Contains(t.CropId))
            .ToList();

        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => cropIds.Contains(h.CropId))
            .ToList();

        var pestRecords = (await _unitOfWork.PestRecords.GetAllAsync(cancellationToken))
            .Where(p => cropIds.Contains(p.CropId))
            .ToList();

        var now = DateTime.UtcNow;
        var currentPeriod = GetMonthToDateRange(now);
        var lastMonthPeriod = GetSamePeriodLastMonth(now);
        var lastYearPeriod = GetSamePeriodLastYear(now);

        var currentStats = CalculatePeriodStats(crops, tasks, harvestRecords, currentPeriod.start, currentPeriod.end);
        var lastMonthStats = CalculatePeriodStats(crops, tasks, harvestRecords, lastMonthPeriod.start, lastMonthPeriod.end);
        var lastYearStats = CalculatePeriodStats(crops, tasks, harvestRecords, lastYearPeriod.start, lastYearPeriod.end);

        var stats = new OverviewStats
        {
            TotalCrops = crops.Count,
            GrowingCrops = crops.Count(c => c.Status == CropStatus.Growing),
            HarvestingCrops = crops.Count(c => c.Status == CropStatus.Harvesting),
            FinishedCrops = crops.Count(c => c.Status == CropStatus.Finished),
            TotalTasks = tasks.Count,
            PendingTasks = tasks.Count(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress),
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
            OverdueTaskCount = tasks.Count(t => (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress) && t.ScheduledDate.Date < DateTime.UtcNow.Date),
            TotalHarvestRecords = harvestRecords.Count,
            TotalHarvestQuantity = harvestRecords.Sum(h => h.Quantity),
            ActivePestIssues = pestRecords.Count(p => p.Status != PestStatus.Resolved),
            ComparedToLastMonth = BuildComparison(currentStats, lastMonthStats),
            ComparedToLastYear = BuildComparison(currentStats, lastYearStats)
        };

        return ApiResponse<OverviewStats>.Success(stats);
    }

    private static (DateTime start, DateTime end) GetMonthToDateRange(DateTime now)
    {
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = now.Date.AddDays(1).AddTicks(-1);
        return (start, end);
    }

    private static (DateTime start, DateTime end) GetSamePeriodLastMonth(DateTime now)
    {
        var lastMonth = now.AddMonths(-1);
        var start = new DateTime(lastMonth.Year, lastMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var day = Math.Min(now.Day, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
        var endDate = new DateTime(lastMonth.Year, lastMonth.Month, day, 0, 0, 0, DateTimeKind.Utc);
        var end = endDate.AddDays(1).AddTicks(-1);
        return (start, end);
    }

    private static (DateTime start, DateTime end) GetSamePeriodLastYear(DateTime now)
    {
        var lastYear = now.AddYears(-1);
        var start = new DateTime(lastYear.Year, lastYear.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var day = Math.Min(now.Day, DateTime.DaysInMonth(lastYear.Year, lastYear.Month));
        var endDate = new DateTime(lastYear.Year, lastYear.Month, day, 0, 0, 0, DateTimeKind.Utc);
        var end = endDate.AddDays(1).AddTicks(-1);
        return (start, end);
    }

    private class PeriodStats
    {
        public int NewCrops { get; set; }
        public decimal HarvestQuantity { get; set; }
        public decimal TaskCompletionRate { get; set; }
    }

    private static PeriodStats CalculatePeriodStats(List<Crop> crops, List<CropCareTask> tasks, List<HarvestRecord> harvestRecords, DateTime start, DateTime end)
    {
        var newCrops = crops.Count(c => c.CreatedAt >= start && c.CreatedAt <= end);
        var harvestQuantity = harvestRecords.Where(h => h.HarvestDate >= start && h.HarvestDate <= end).Sum(h => h.Quantity);

        var periodTasks = tasks.Where(t => t.ScheduledDate >= start && t.ScheduledDate <= end).ToList();
        var effectiveTasks = periodTasks.Where(t => t.Status != TaskStatus.Cancelled).ToList();
        var totalTasks = effectiveTasks.Count;
        var completedTasks = effectiveTasks.Count(t => t.Status == TaskStatus.Completed);
        var completionRate = totalTasks > 0 ? Math.Round((decimal)completedTasks / totalTasks * 100, 1) : 0m;

        return new PeriodStats
        {
            NewCrops = newCrops,
            HarvestQuantity = harvestQuantity,
            TaskCompletionRate = completionRate
        };
    }

    private static OverviewComparison BuildComparison(PeriodStats current, PeriodStats previous)
    {
        return new OverviewComparison
        {
            NewCrops = BuildIntComparison(current.NewCrops, previous.NewCrops),
            HarvestQuantity = BuildDecimalComparison(current.HarvestQuantity, previous.HarvestQuantity),
            TaskCompletionRate = BuildDecimalComparison(current.TaskCompletionRate, previous.TaskCompletionRate)
        };
    }

    private static PeriodComparison<int> BuildIntComparison(int current, int previous)
    {
        var change = current - previous;
        decimal? percentage = previous > 0 ? Math.Round((decimal)change / previous * 100, 1) : null;
        return new PeriodComparison<int>
        {
            CurrentValue = current,
            PreviousValue = previous,
            Change = change,
            ChangePercentage = percentage
        };
    }

    private static PeriodComparison<decimal> BuildDecimalComparison(decimal current, decimal previous)
    {
        var change = Math.Round(current - previous, 1);
        decimal? percentage = previous > 0 ? Math.Round(change / previous * 100, 1) : null;
        return new PeriodComparison<decimal>
        {
            CurrentValue = Math.Round(current, 1),
            PreviousValue = Math.Round(previous, 1),
            Change = change,
            ChangePercentage = percentage
        };
    }

    public async Task<ApiResponse<IEnumerable<TrendData>>> GetTrendStatsAsync(DateTime startDate, DateTime endDate, Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户趋势统计: {UserId}, 开始日期: {StartDate}, 结束日期: {EndDate}, 位置: {LocationId}", userId, startDate, endDate, plantingLocationId);

        if (startDate > endDate)
        {
            return ApiResponse<IEnumerable<TrendData>>.Error("开始日期不能大于结束日期", 400);
        }

        var cropsQuery = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).AsEnumerable();

        if (plantingLocationId.HasValue)
        {
            cropsQuery = cropsQuery.Where(c => c.PlantingLocationId == plantingLocationId.Value);
        }

        var cropIds = cropsQuery.Select(c => c.Id).ToList();

        var crops = (await _unitOfWork.Crops.FindAsync(c => cropIds.Contains(c.Id), cancellationToken)).ToList();
        var tasks = (await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken))
            .Where(t => cropIds.Contains(t.CropId) && t.CompletedDate.HasValue)
            .ToList();
        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => cropIds.Contains(h.CropId))
            .ToList();

        var dateSpan = endDate - startDate;
        var isWeekly = dateSpan.TotalDays > 31;

        var trendData = new List<TrendData>();

        if (isWeekly)
        {
            var startOfWeek = StartOfWeek(startDate, DayOfWeek.Monday);
            while (startOfWeek <= endDate)
            {
                var weekEnd = startOfWeek.AddDays(7).AddTicks(-1);

                var newCrops = crops.Count(c =>
                    c.CreatedAt.Date >= startOfWeek.Date && c.CreatedAt.Date <= weekEnd.Date);

                var completedTasks = tasks.Count(t =>
                    t.CompletedDate!.Value.Date >= startOfWeek.Date && t.CompletedDate!.Value.Date <= weekEnd.Date);

                var weekHarvests = harvestRecords.Where(h =>
                    h.HarvestDate.Date >= startOfWeek.Date && h.HarvestDate.Date <= weekEnd.Date).ToList();

                trendData.Add(new TrendData
                {
                    Period = $"{startOfWeek:yyyy-MM-dd} ~ {weekEnd:yyyy-MM-dd}",
                    NewCrops = newCrops,
                    CompletedTasks = completedTasks,
                    HarvestRecords = weekHarvests.Count,
                    HarvestQuantity = weekHarvests.Sum(h => h.Quantity)
                });

                startOfWeek = startOfWeek.AddDays(7);
            }
        }
        else
        {
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var newCrops = crops.Count(c => c.CreatedAt.Date == date);
                var completedTasks = tasks.Count(t => t.CompletedDate!.Value.Date == date);
                var dayHarvests = harvestRecords.Where(h => h.HarvestDate.Date == date).ToList();

                trendData.Add(new TrendData
                {
                    Period = date.ToString("yyyy-MM-dd"),
                    NewCrops = newCrops,
                    CompletedTasks = completedTasks,
                    HarvestRecords = dayHarvests.Count,
                    HarvestQuantity = dayHarvests.Sum(h => h.Quantity)
                });
            }
        }

        return ApiResponse<IEnumerable<TrendData>>.Success(trendData);
    }

    public async Task<ApiResponse<IEnumerable<CropTaskCompletionItem>>> GetCropTaskCompletionStatsAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取作物任务完成率统计: {UserId}, 位置: {LocationId}", userId, plantingLocationId);

        var crops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();

        if (plantingLocationId.HasValue)
        {
            crops = crops.Where(c => c.PlantingLocationId == plantingLocationId.Value).ToList();
        }
        if (!crops.Any())
        {
            return ApiResponse<IEnumerable<CropTaskCompletionItem>>.Success(Enumerable.Empty<CropTaskCompletionItem>());
        }

        var cropIds = crops.Select(c => c.Id).ToList();
        var allTasks = (await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken))
            .Where(t => cropIds.Contains(t.CropId))
            .ToList();

        var result = crops.Select(crop =>
        {
            var cropTasks = allTasks.Where(t => t.CropId == crop.Id).ToList();
            var effectiveTasks = cropTasks.Where(t => t.Status != TaskStatus.Cancelled).ToList();
            var completed = effectiveTasks.Count(t => t.Status == TaskStatus.Completed);
            var onTimeCompleted = effectiveTasks.Count(t =>
                t.Status == TaskStatus.Completed &&
                t.CompletedDate.HasValue &&
                t.CompletedDate.Value.Date <= t.ScheduledDate.Date);
            var overdue = effectiveTasks.Count(t =>
                (t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress) &&
                t.ScheduledDate.Date < DateTime.UtcNow.Date);

            var total = effectiveTasks.Count;

            return new CropTaskCompletionItem
            {
                CropId = crop.Id,
                CropName = crop.Name,
                TotalTasks = total,
                CompletedTasks = completed,
                OnTimeCompletedTasks = onTimeCompleted,
                OverdueTasks = overdue,
                CompletionRate = total > 0 ? Math.Round((decimal)completed / total * 100, 1) : 0,
                OnTimeRate = total > 0 ? Math.Round((decimal)onTimeCompleted / total * 100, 1) : 0
            };
        }).ToList();

        return ApiResponse<IEnumerable<CropTaskCompletionItem>>.Success(result);
    }

    public async Task<ApiResponse<HarvestQualityAnalysis>> GetHarvestQualityAnalysisAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户收成质量分析: {UserId}, 位置: {LocationId}", userId, plantingLocationId);

        var crops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();

        if (plantingLocationId.HasValue)
        {
            crops = crops.Where(c => c.PlantingLocationId == plantingLocationId.Value).ToList();
        }
        if (!crops.Any())
        {
            return ApiResponse<HarvestQualityAnalysis>.Success(new HarvestQualityAnalysis());
        }

        var cropIds = crops.Select(c => c.Id).ToList();
        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => cropIds.Contains(h.CropId))
            .ToList();

        if (!harvestRecords.Any())
        {
            return ApiResponse<HarvestQualityAnalysis>.Success(new HarvestQualityAnalysis());
        }

        var cropDict = crops.ToDictionary(c => c.Id);
        var totalRecords = harvestRecords.Count;
        var highQualityThreshold = HarvestQuality.Good;

        var qualityDistribution = GetQualityDistribution(harvestRecords, totalRecords);
        var byCrop = GetCropQualityStats(harvestRecords, cropDict, highQualityThreshold);
        var byLocation = GetLocationQualityStats(harvestRecords, cropDict, highQualityThreshold);
        var bySeason = GetSeasonQualityStats(harvestRecords, highQualityThreshold);
        var byContainer = GetContainerQualityStats(harvestRecords, cropDict, highQualityThreshold);
        var insights = GenerateInsights(byCrop, byLocation, bySeason, byContainer);

        var overallAvgScore = (decimal)harvestRecords.Average(h => (int)h.Quality);
        var overallHighQualityRate = harvestRecords.Count(h => h.Quality >= highQualityThreshold) / (decimal)totalRecords * 100;

        var analysis = new HarvestQualityAnalysis
        {
            QualityDistribution = qualityDistribution,
            ByCrop = byCrop.OrderByDescending(c => c.HighQualityRate).ToList(),
            ByLocation = byLocation.OrderByDescending(l => l.HighQualityRate).ToList(),
            BySeason = bySeason.OrderByDescending(s => s.HighQualityRate).ToList(),
            ByContainer = byContainer.OrderByDescending(c => c.HighQualityRate).ToList(),
            Insights = insights,
            OverallAverageQualityScore = Math.Round(overallAvgScore, 2),
            OverallHighQualityRate = Math.Round(overallHighQualityRate, 1),
            TotalHarvestRecords = totalRecords
        };

        return ApiResponse<HarvestQualityAnalysis>.Success(analysis);
    }

    private static List<QualityDistributionItem> GetQualityDistribution(List<HarvestRecord> records, int totalRecords)
    {
        var qualities = Enum.GetValues(typeof(HarvestQuality)).Cast<HarvestQuality>().ToList();
        var distribution = new List<QualityDistributionItem>();

        foreach (var quality in qualities)
        {
            var count = records.Count(h => h.Quality == quality);
            var percentage = totalRecords > 0 ? Math.Round((decimal)count / totalRecords * 100, 1) : 0;
            var totalQuantity = records.Where(h => h.Quality == quality).Sum(h => h.Quantity);

            distribution.Add(new QualityDistributionItem
            {
                Quality = quality,
                QualityName = GetQualityName(quality),
                Count = count,
                Percentage = percentage,
                TotalQuantity = Math.Round(totalQuantity, 2)
            });
        }

        return distribution;
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

    private static List<CropQualityStats> GetCropQualityStats(List<HarvestRecord> records, Dictionary<Guid, Crop> cropDict, HarvestQuality highQualityThreshold)
    {
        return records
            .GroupBy(h => h.CropId)
            .Select(g =>
            {
                var crop = cropDict.GetValueOrDefault(g.Key);
                var harvests = g.ToList();
                var highQualityCount = harvests.Count(h => h.Quality >= highQualityThreshold);
                var avgScore = (decimal)harvests.Average(h => (int)h.Quality);

                return new CropQualityStats
                {
                    CropId = g.Key,
                    CropName = crop?.Name ?? "未知作物",
                    TotalHarvests = harvests.Count,
                    HighQualityHarvests = highQualityCount,
                    HighQualityRate = Math.Round((decimal)highQualityCount / harvests.Count * 100, 1),
                    AverageQualityScore = Math.Round(avgScore, 2),
                    TotalQuantity = Math.Round(harvests.Sum(h => h.Quantity), 2)
                };
            })
            .ToList();
    }

    private static List<LocationQualityStats> GetLocationQualityStats(List<HarvestRecord> records, Dictionary<Guid, Crop> cropDict, HarvestQuality highQualityThreshold)
    {
        return records
            .GroupBy(h => cropDict.GetValueOrDefault(h.CropId)?.Location ?? "未知位置")
            .Select(g =>
            {
                var harvests = g.ToList();
                var highQualityCount = harvests.Count(h => h.Quality >= highQualityThreshold);
                var avgScore = (decimal)harvests.Average(h => (int)h.Quality);
                var cropCount = harvests.Select(h => h.CropId).Distinct().Count();

                return new LocationQualityStats
                {
                    Location = g.Key,
                    TotalHarvests = harvests.Count,
                    HighQualityHarvests = highQualityCount,
                    HighQualityRate = Math.Round((decimal)highQualityCount / harvests.Count * 100, 1),
                    AverageQualityScore = Math.Round(avgScore, 2),
                    CropCount = cropCount
                };
            })
            .ToList();
    }

    private static List<SeasonQualityStats> GetSeasonQualityStats(List<HarvestRecord> records, HarvestQuality highQualityThreshold)
    {
        return records
            .GroupBy(h => GetSeason(h.HarvestDate))
            .Select(g =>
            {
                var harvests = g.ToList();
                var highQualityCount = harvests.Count(h => h.Quality >= highQualityThreshold);
                var avgScore = (decimal)harvests.Average(h => (int)h.Quality);

                return new SeasonQualityStats
                {
                    Season = g.Key,
                    TotalHarvests = harvests.Count,
                    HighQualityHarvests = highQualityCount,
                    HighQualityRate = Math.Round((decimal)highQualityCount / harvests.Count * 100, 1),
                    AverageQualityScore = Math.Round(avgScore, 2),
                    TotalQuantity = Math.Round(harvests.Sum(h => h.Quantity), 2)
                };
            })
            .ToList();
    }

    private static string GetSeason(DateTime date)
    {
        int month = date.Month;
        return month switch
        {
            3 or 4 or 5 => "春季",
            6 or 7 or 8 => "夏季",
            9 or 10 or 11 => "秋季",
            12 or 1 or 2 => "冬季",
            _ => "未知"
        };
    }

    private static List<ContainerQualityStats> GetContainerQualityStats(List<HarvestRecord> records, Dictionary<Guid, Crop> cropDict, HarvestQuality highQualityThreshold)
    {
        return records
            .GroupBy(h => cropDict.GetValueOrDefault(h.CropId)?.ContainerType ?? "未知容器")
            .Select(g =>
            {
                var harvests = g.ToList();
                var highQualityCount = harvests.Count(h => h.Quality >= highQualityThreshold);
                var avgScore = (decimal)harvests.Average(h => (int)h.Quality);

                return new ContainerQualityStats
                {
                    ContainerType = g.Key,
                    TotalHarvests = harvests.Count,
                    HighQualityHarvests = highQualityCount,
                    HighQualityRate = Math.Round((decimal)highQualityCount / harvests.Count * 100, 1),
                    AverageQualityScore = Math.Round(avgScore, 2)
                };
            })
            .ToList();
    }

    private static List<QualityInsight> GenerateInsights(
        List<CropQualityStats> byCrop,
        List<LocationQualityStats> byLocation,
        List<SeasonQualityStats> bySeason,
        List<ContainerQualityStats> byContainer)
    {
        var insights = new List<QualityInsight>();

        if (byCrop.Count >= 2)
        {
            var topCrop = byCrop.OrderByDescending(c => c.HighQualityRate).First();
            var bottomCrop = byCrop.OrderBy(c => c.HighQualityRate).First();

            var recommendations = new List<string>
            {
                $"「{topCrop.CropName}」优质率达 {topCrop.HighQualityRate}%，表现最佳，可考虑扩大种植",
                $"「{bottomCrop.CropName}」优质率仅 {bottomCrop.HighQualityRate}%，建议优化种植条件或调整品种"
            };

            insights.Add(new QualityInsight
            {
                Dimension = "作物品种",
                TopPerformer = topCrop.CropName,
                TopHighQualityRate = topCrop.HighQualityRate,
                BottomPerformer = bottomCrop.CropName,
                BottomHighQualityRate = bottomCrop.HighQualityRate,
                Recommendations = recommendations
            });
        }

        if (byLocation.Count >= 2)
        {
            var topLocation = byLocation.OrderByDescending(l => l.HighQualityRate).First();
            var bottomLocation = byLocation.OrderBy(l => l.HighQualityRate).First();

            var recommendations = new List<string>
            {
                $"「{topLocation.Location}」优质率最高（{topLocation.HighQualityRate}%），光照和通风条件可能最佳",
                $"「{bottomLocation.Location}」优质率偏低（{bottomLocation.HighQualityRate}%），建议检查光照、通风等环境条件",
                "优先在优质率高的位置种植高价值作物"
            };

            insights.Add(new QualityInsight
            {
                Dimension = "种植位置",
                TopPerformer = topLocation.Location,
                TopHighQualityRate = topLocation.HighQualityRate,
                BottomPerformer = bottomLocation.Location,
                BottomHighQualityRate = bottomLocation.HighQualityRate,
                Recommendations = recommendations
            });
        }

        if (bySeason.Count >= 2)
        {
            var topSeason = bySeason.OrderByDescending(s => s.HighQualityRate).First();
            var bottomSeason = bySeason.OrderBy(s => s.HighQualityRate).First();

            var recommendations = new List<string>
            {
                $"「{topSeason.Season}」是收成质量最佳的季节，优质率 {topSeason.HighQualityRate}%",
                $"「{bottomSeason.Season}」收成质量相对较低，建议选择适合当季的作物品种",
                "根据季节特点调整种植计划，提高整体收成质量"
            };

            insights.Add(new QualityInsight
            {
                Dimension = "季节",
                TopPerformer = topSeason.Season,
                TopHighQualityRate = topSeason.HighQualityRate,
                BottomPerformer = bottomSeason.Season,
                BottomHighQualityRate = bottomSeason.HighQualityRate,
                Recommendations = recommendations
            });
        }

        if (byContainer.Count >= 2)
        {
            var topContainer = byContainer.OrderByDescending(c => c.HighQualityRate).First();
            var bottomContainer = byContainer.OrderBy(c => c.HighQualityRate).First();

            var recommendations = new List<string>
            {
                $"「{topContainer.ContainerType}」种植的收成质量更好，优质率 {topContainer.HighQualityRate}%",
                $"「{bottomContainer.ContainerType}」的表现较差，可能需要更换容器或改善排水",
                "优质作物建议优先使用表现好的容器类型"
            };

            insights.Add(new QualityInsight
            {
                Dimension = "容器类型",
                TopPerformer = topContainer.ContainerType,
                TopHighQualityRate = topContainer.HighQualityRate,
                BottomPerformer = bottomContainer.ContainerType,
                BottomHighQualityRate = bottomContainer.HighQualityRate,
                Recommendations = recommendations
            });
        }

        return insights;
    }

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
