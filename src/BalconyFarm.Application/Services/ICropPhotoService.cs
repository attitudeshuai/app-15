using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface ICropPhotoService
{
    Task<ApiResponse<PagedResult<CropPhotoDto>>> GetPhotosAsync(CropPhotoQueryRequestDto query, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CropPhotoDto>> GetPhotoByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<CropPhotoDto>> CreatePhotoAsync(CreateCropPhotoRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CropPhotoDto>> UpdatePhotoAsync(Guid id, UpdateCropPhotoRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeletePhotoAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CropGrowthTimelineDto>> GetGrowthTimelineAsync(Guid cropId, Guid? userId = null, CancellationToken cancellationToken = default);
}
