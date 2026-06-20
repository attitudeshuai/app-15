using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IReportService
{
    Task<ApiResponse<PlantingReport>> GetMonthlyReportAsync(Guid userId, int year, int month, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingReport>> GetYearlyReportAsync(Guid userId, int year, Guid? plantingLocationId = null, CancellationToken cancellationToken = default);
}
