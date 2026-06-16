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

    public async Task<ApiResponse<OverviewStats>> GetOverviewStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户总览统计: {UserId}", userId);

        var crops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
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

        var stats = new OverviewStats
        {
            TotalCrops = crops.Count,
            GrowingCrops = crops.Count(c => c.Status == CropStatus.Growing),
            HarvestingCrops = crops.Count(c => c.Status == CropStatus.Harvesting),
            FinishedCrops = crops.Count(c => c.Status == CropStatus.Finished),
            TotalTasks = tasks.Count,
            PendingTasks = tasks.Count(t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress),
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
            TotalHarvestRecords = harvestRecords.Count,
            TotalHarvestQuantity = harvestRecords.Sum(h => h.Quantity),
            ActivePestIssues = pestRecords.Count(p => p.Status != PestStatus.Resolved)
        };

        return ApiResponse<OverviewStats>.Success(stats);
    }

    public async Task<ApiResponse<IEnumerable<TrendData>>> GetTrendStatsAsync(DateTime startDate, DateTime endDate, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户趋势统计: {UserId}, 开始日期: {StartDate}, 结束日期: {EndDate}", userId, startDate, endDate);

        if (startDate > endDate)
        {
            return ApiResponse<IEnumerable<TrendData>>.Error("开始日期不能大于结束日期", 400);
        }

        var cropIds = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken))
            .Select(c => c.Id)
            .ToList();

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

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
}
