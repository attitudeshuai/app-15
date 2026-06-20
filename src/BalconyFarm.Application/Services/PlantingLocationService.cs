using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class PlantingLocationService : IPlantingLocationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlantingLocationService> _logger;

    public PlantingLocationService(IUnitOfWork unitOfWork, ILogger<PlantingLocationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<PlantingLocationDto>>> GetPlantingLocationsAsync(Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var locations = (await _unitOfWork.PlantingLocations.GetAllAsync(cancellationToken)).AsEnumerable();

        if (userId.HasValue)
        {
            locations = locations.Where(l => l.UserId == userId.Value);
        }

        locations = locations.OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt);

        var locationDtos = locations.Select(EnrichWithCropCount).ToList();

        return ApiResponse<IEnumerable<PlantingLocationDto>>.Success(locationDtos);
    }

    public async Task<ApiResponse<PlantingLocationDto>> GetPlantingLocationByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var location = await _unitOfWork.PlantingLocations.GetByIdAsync(id, cancellationToken);
        if (location == null)
        {
            return ApiResponse<PlantingLocationDto>.Error("种植位置不存在", 404);
        }

        if (userId.HasValue && location.UserId != userId.Value)
        {
            return ApiResponse<PlantingLocationDto>.Error("无权访问此种植位置", 403);
        }

        var locationDto = EnrichWithCropCount(location);
        return ApiResponse<PlantingLocationDto>.Success(locationDto);
    }

    public async Task<ApiResponse<PlantingLocationDto>> CreatePlantingLocationAsync(CreatePlantingLocationRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建种植位置: {Name}, 用户: {UserId}", dto.Name, userId);

        var location = dto.Adapt<PlantingLocation>();
        location.Id = Guid.NewGuid();
        location.UserId = userId;
        location.CreatedAt = DateTime.UtcNow;
        location.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PlantingLocations.AddAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种植位置创建成功: {LocationId}", location.Id);

        var locationDto = EnrichWithCropCount(location);
        return ApiResponse<PlantingLocationDto>.Success(locationDto, "创建成功");
    }

    public async Task<ApiResponse<PlantingLocationDto>> UpdatePlantingLocationAsync(Guid id, UpdatePlantingLocationRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新种植位置: {LocationId}, 用户: {UserId}", id, userId);

        var location = await _unitOfWork.PlantingLocations.GetByIdAsync(id, cancellationToken);
        if (location == null)
        {
            return ApiResponse<PlantingLocationDto>.Error("种植位置不存在", 404);
        }

        if (location.UserId != userId)
        {
            return ApiResponse<PlantingLocationDto>.Error("无权修改此种植位置", 403);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            location.Name = dto.Name;
        if (dto.Description != null)
            location.Description = dto.Description;
        if (dto.LocationType != null)
            location.LocationType = dto.LocationType;
        if (dto.SunlightCondition != null)
            location.SunlightCondition = dto.SunlightCondition;
        if (dto.Area.HasValue)
            location.Area = dto.Area.Value;
        if (dto.PhotoUrl != null)
            location.PhotoUrl = dto.PhotoUrl;
        if (dto.SortOrder.HasValue)
            location.SortOrder = dto.SortOrder.Value;

        location.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.PlantingLocations.UpdateAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种植位置更新成功: {LocationId}", id);

        var locationDto = EnrichWithCropCount(location);
        return ApiResponse<PlantingLocationDto>.Success(locationDto, "更新成功");
    }

    public async Task<ApiResponse> DeletePlantingLocationAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除种植位置: {LocationId}, 用户: {UserId}", id, userId);

        var location = await _unitOfWork.PlantingLocations.GetByIdAsync(id, cancellationToken);
        if (location == null)
        {
            return ApiResponse.Error("种植位置不存在", 404);
        }

        if (location.UserId != userId)
        {
            return ApiResponse.Error("无权删除此种植位置", 403);
        }

        var cropsInLocation = (await _unitOfWork.Crops.FindAsync(c => c.PlantingLocationId == id, cancellationToken)).ToList();
        if (cropsInLocation.Any())
        {
            return ApiResponse.Error("该位置下还有作物，无法删除，请先移动或删除作物", 400);
        }

        await _unitOfWork.PlantingLocations.DeleteAsync(location, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种植位置删除成功: {LocationId}", id);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<IEnumerable<PlantingLocationStatsDto>>> GetLocationStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("获取用户位置统计: {UserId}", userId);

        var locations = (await _unitOfWork.PlantingLocations.FindAsync(l => l.UserId == userId, cancellationToken)).ToList();
        var allCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        var allCropIds = allCrops.Select(c => c.Id).ToList();

        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => allCropIds.Contains(h.CropId))
            .ToList();

        var pestRecords = (await _unitOfWork.PestRecords.GetAllAsync(cancellationToken))
            .Where(p => allCropIds.Contains(p.CropId))
            .ToList();

        var stats = locations.Select(location =>
        {
            var crops = allCrops.Where(c => c.PlantingLocationId == location.Id).ToList();
            var cropIds = crops.Select(c => c.Id).ToList();
            var locationHarvests = harvestRecords.Where(h => cropIds.Contains(h.CropId)).ToList();
            var locationPests = pestRecords.Where(p => cropIds.Contains(p.CropId)).ToList();

            return new PlantingLocationStatsDto
            {
                LocationId = location.Id,
                LocationName = location.Name,
                TotalCrops = crops.Count,
                GrowingCrops = crops.Count(c => c.Status == CropStatus.Growing),
                HarvestingCrops = crops.Count(c => c.Status == CropStatus.Harvesting),
                FinishedCrops = crops.Count(c => c.Status == CropStatus.Finished),
                TotalHarvestRecords = locationHarvests.Count,
                TotalHarvestQuantity = locationHarvests.Sum(h => h.Quantity),
                ActivePestIssues = locationPests.Count(p => p.Status != PestStatus.Resolved)
            };
        }).ToList();

        return ApiResponse<IEnumerable<PlantingLocationStatsDto>>.Success(stats);
    }

    public async Task<ApiResponse<IEnumerable<PlantingLocationDto>>> GetMyPlantingLocationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetPlantingLocationsAsync(userId, cancellationToken);
    }

    private static PlantingLocationDto EnrichWithCropCount(PlantingLocation location)
    {
        var dto = location.Adapt<PlantingLocationDto>();
        dto.CropCount = location.Crops?.Count ?? 0;
        return dto;
    }
}
