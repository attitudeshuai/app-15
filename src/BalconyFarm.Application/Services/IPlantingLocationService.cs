using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IPlantingLocationService
{
    Task<ApiResponse<IEnumerable<PlantingLocationDto>>> GetPlantingLocationsAsync(Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingLocationDto>> GetPlantingLocationByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingLocationDto>> CreatePlantingLocationAsync(CreatePlantingLocationRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingLocationDto>> UpdatePlantingLocationAsync(Guid id, UpdatePlantingLocationRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeletePlantingLocationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PlantingLocationStatsDto>>> GetLocationStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<PlantingLocationDto>>> GetMyPlantingLocationsAsync(Guid userId, CancellationToken cancellationToken = default);
}
