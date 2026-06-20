using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IGlobalSearchService
{
    Task<ApiResponse<GlobalSearchResultDto>> SearchAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken = default);
}
