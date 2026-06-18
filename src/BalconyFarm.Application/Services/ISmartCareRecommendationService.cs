using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface ISmartCareRecommendationService
{
    Task<ApiResponse<GenerateCareTasksResultDto>> GenerateCareTasksAsync(GenerateCareTasksRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GenerateCareTasksResultDto>> PreviewCareTasksAsync(GenerateCareTasksRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
}
