using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IWeatherAwareTaskAdjustmentService
{
    Task<ApiResponse<WeatherAdjustTaskResultDto>> AdjustUpcomingWateringTasksAsync(
        WeatherAdjustTaskRequestDto dto,
        Guid userId,
        CancellationToken cancellationToken = default);
}
