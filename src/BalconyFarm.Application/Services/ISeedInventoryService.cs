using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface ISeedInventoryService
{
    Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetSeedInventoriesAsync(SeedInventoryQueryRequestDto query, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<SeedInventoryDto>> GetSeedInventoryByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<SeedInventoryDto>> CreateSeedInventoryAsync(CreateSeedInventoryRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SeedInventoryDto>> UpdateSeedInventoryAsync(Guid id, UpdateSeedInventoryRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteSeedInventoryAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SeedInventoryDto>> UseSeedAsync(Guid id, UseSeedRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetMySeedInventoriesAsync(SeedInventoryQueryRequestDto query, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetExpiringSeedsAsync(int daysThreshold, Guid userId, CancellationToken cancellationToken = default);
}
