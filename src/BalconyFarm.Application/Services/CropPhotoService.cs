using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class CropPhotoService : ICropPhotoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CropPhotoService> _logger;

    public CropPhotoService(IUnitOfWork unitOfWork, ILogger<CropPhotoService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<CropPhotoDto>>> GetPhotosAsync(CropPhotoQueryRequestDto query, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var photos = (await _unitOfWork.CropPhotos.GetAllAsync(cancellationToken)).ToList();
        var crops = (await _unitOfWork.Crops.GetAllAsync(cancellationToken)).ToList();

        IEnumerable<CropPhoto> photosQuery = photos;

        if (userId.HasValue)
        {
            var userCropIds = crops.Where(c => c.UserId == userId.Value).Select(c => c.Id).ToList();
            photosQuery = photosQuery.Where(p => userCropIds.Contains(p.CropId));
        }

        if (query.CropId.HasValue)
        {
            photosQuery = photosQuery.Where(p => p.CropId == query.CropId.Value);
        }

        if (query.PhotoDateFrom.HasValue)
        {
            photosQuery = photosQuery.Where(p => p.PhotoDate >= query.PhotoDateFrom.Value);
        }

        if (query.PhotoDateTo.HasValue)
        {
            photosQuery = photosQuery.Where(p => p.PhotoDate <= query.PhotoDateTo.Value);
        }

        var totalCount = photosQuery.Count();
        var sortFunc = GetSortProperty(query.SortBy ?? "photodate").Compile();

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            photosQuery = query.SortOrder?.ToLower() == "desc"
                ? photosQuery.OrderByDescending(sortFunc)
                : photosQuery.OrderBy(sortFunc);
        }
        else
        {
            photosQuery = photosQuery.OrderBy(p => p.PhotoDate);
        }

        var items = photosQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p =>
            {
                var dto = p.Adapt<CropPhotoDto>();
                var crop = crops.FirstOrDefault(c => c.Id == p.CropId);
                dto.CropName = crop?.Name;
                dto.DaysAfterPlanting = crop != null ? (int)(p.PhotoDate.Date - crop.PlantingDate.Date).TotalDays : null;
                return dto;
            })
            .ToList();

        var result = new PagedResult<CropPhotoDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PagedResult<CropPhotoDto>>.Success(result);
    }

    public async Task<ApiResponse<CropPhotoDto>> GetPhotoByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var photo = (await _unitOfWork.CropPhotos.GetAllAsync(cancellationToken))
            .FirstOrDefault(p => p.Id == id);

        if (photo == null)
        {
            return ApiResponse<CropPhotoDto>.Error("照片记录不存在", 404);
        }

        var crop = await _unitOfWork.Crops.GetByIdAsync(photo.CropId, cancellationToken);

        if (userId.HasValue && crop != null && crop.UserId != userId.Value)
        {
            return ApiResponse<CropPhotoDto>.Error("无权访问此照片记录", 403);
        }

        var photoDto = photo.Adapt<CropPhotoDto>();
        photoDto.CropName = crop?.Name;
        photoDto.DaysAfterPlanting = crop != null ? (int)(photo.PhotoDate.Date - crop.PlantingDate.Date).TotalDays : null;
        return ApiResponse<CropPhotoDto>.Success(photoDto);
    }

    public async Task<ApiResponse<CropPhotoDto>> CreatePhotoAsync(CreateCropPhotoRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建作物照片记录: 作物ID: {CropId}, 用户: {UserId}", dto.CropId, userId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropPhotoDto>.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse<CropPhotoDto>.Error("无权为此作物创建照片记录", 403);
        }

        if (dto.PhotoDate < crop.PlantingDate)
        {
            return ApiResponse<CropPhotoDto>.Error("拍摄日期不能早于种植日期", 400);
        }

        var photo = dto.Adapt<CropPhoto>();
        photo.Id = Guid.NewGuid();
        photo.DaysAfterPlanting = (int)(dto.PhotoDate.Date - crop.PlantingDate.Date).TotalDays;

        await _unitOfWork.CropPhotos.AddAsync(photo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物照片记录创建成功: {PhotoId}", photo.Id);

        var photoDto = photo.Adapt<CropPhotoDto>();
        photoDto.CropName = crop.Name;
        return ApiResponse<CropPhotoDto>.Success(photoDto, "创建成功");
    }

    public async Task<ApiResponse<CropPhotoDto>> UpdatePhotoAsync(Guid id, UpdateCropPhotoRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新作物照片记录: {PhotoId}, 用户: {UserId}", id, userId);

        var photo = (await _unitOfWork.CropPhotos.GetAllAsync(cancellationToken))
            .FirstOrDefault(p => p.Id == id);

        if (photo == null)
        {
            return ApiResponse<CropPhotoDto>.Error("照片记录不存在", 404);
        }

        var crop = await _unitOfWork.Crops.GetByIdAsync(photo.CropId, cancellationToken);
        if (crop != null && crop.UserId != userId)
        {
            return ApiResponse<CropPhotoDto>.Error("无权修改此照片记录", 403);
        }

        if (dto.PhotoDate.HasValue && crop != null && dto.PhotoDate.Value < crop.PlantingDate)
        {
            return ApiResponse<CropPhotoDto>.Error("拍摄日期不能早于种植日期", 400);
        }

        if (dto.PhotoDate.HasValue)
        {
            photo.PhotoDate = dto.PhotoDate.Value;
            if (crop != null)
            {
                photo.DaysAfterPlanting = (int)(dto.PhotoDate.Value.Date - crop.PlantingDate.Date).TotalDays;
            }
        }
        if (!string.IsNullOrEmpty(dto.PhotoUrl))
            photo.PhotoUrl = dto.PhotoUrl;
        if (dto.Description != null)
            photo.Description = dto.Description;

        await _unitOfWork.CropPhotos.UpdateAsync(photo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物照片记录更新成功: {PhotoId}", id);

        var photoDto = photo.Adapt<CropPhotoDto>();
        photoDto.CropName = crop?.Name;
        photoDto.DaysAfterPlanting = photo.DaysAfterPlanting;
        return ApiResponse<CropPhotoDto>.Success(photoDto, "更新成功");
    }

    public async Task<ApiResponse> DeletePhotoAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除作物照片记录: {PhotoId}, 用户: {UserId}", id, userId);

        var photo = (await _unitOfWork.CropPhotos.GetAllAsync(cancellationToken))
            .FirstOrDefault(p => p.Id == id);

        if (photo == null)
        {
            return ApiResponse.Error("照片记录不存在", 404);
        }

        var crop = await _unitOfWork.Crops.GetByIdAsync(photo.CropId, cancellationToken);
        if (crop != null && crop.UserId != userId)
        {
            return ApiResponse.Error("无权删除此照片记录", 403);
        }

        await _unitOfWork.CropPhotos.DeleteAsync(photo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物照片记录删除成功: {PhotoId}", id);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<CropGrowthTimelineDto>> GetGrowthTimelineAsync(Guid cropId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取作物成长时间轴: 作物ID: {CropId}", cropId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(cropId, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropGrowthTimelineDto>.Error("作物不存在", 404);
        }

        if (userId.HasValue && crop.UserId != userId.Value)
        {
            return ApiResponse<CropGrowthTimelineDto>.Error("无权访问此作物的成长时间轴", 403);
        }

        var allPhotos = (await _unitOfWork.CropPhotos.GetAllAsync(cancellationToken))
            .Where(p => p.CropId == cropId)
            .OrderBy(p => p.PhotoDate)
            .ToList();

        var totalDays = (int)(DateTime.UtcNow.Date - crop.PlantingDate.Date).TotalDays;

        var photosByDate = allPhotos
            .GroupBy(p => p.PhotoDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new CropPhotoTimelineDto
            {
                Date = g.Key,
                Photos = g.Select(p =>
                {
                    var dto = p.Adapt<CropPhotoDto>();
                    dto.CropName = crop.Name;
                    dto.DaysAfterPlanting = (int)(p.PhotoDate.Date - crop.PlantingDate.Date).TotalDays;
                    return dto;
                }).ToList()
            })
            .ToList();

        var timelineDto = new CropGrowthTimelineDto
        {
            CropId = crop.Id,
            CropName = crop.Name,
            Variety = crop.Variety,
            PlantingDate = crop.PlantingDate,
            TotalDays = totalDays,
            PhotoCount = allPhotos.Count,
            Timeline = photosByDate
        };

        return ApiResponse<CropGrowthTimelineDto>.Success(timelineDto);
    }

    private static System.Linq.Expressions.Expression<Func<CropPhoto, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "photodate" => photo => photo.PhotoDate,
            "cropid" => photo => photo.CropId,
            "createdat" => photo => photo.CreatedAt,
            _ => photo => photo.PhotoDate
        };
    }
}
