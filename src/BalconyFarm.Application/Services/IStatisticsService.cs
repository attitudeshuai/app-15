using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IStatisticsService
{
    Task<ApiResponse<OverviewStats>> GetOverviewStatsAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<TrendData>>> GetTrendStatsAsync(DateTime startDate, DateTime endDate, Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<CropTaskCompletionItem>>> GetCropTaskCompletionStatsAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<HarvestQualityAnalysis>> GetHarvestQualityAnalysisAsync(Guid userId, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
}
