using BalconyFarm.Application.Data;
using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class PlantingCalendarService : IPlantingCalendarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlantingCalendarService> _logger;

    public PlantingCalendarService(IUnitOfWork unitOfWork, ILogger<PlantingCalendarService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CityDto>>> GetAvailableCitiesAsync(CancellationToken cancellationToken = default)
    {
        var cities = PlantingCalendarData.CityClimateData.Values
            .Select(c => new CityDto
            {
                Name = c.Name,
                Province = c.Province,
                ClimateZone = c.ClimateZone
            })
            .OrderBy(c => c.Province)
            .ThenBy(c => c.Name)
            .ToList();

        return await Task.FromResult(ApiResponse<List<CityDto>>.Success(cities));
    }

    public async Task<ApiResponse<PlantingCalendarResponseDto>> GetRecommendationsAsync(GetRecommendationsRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.City))
        {
            return ApiResponse<PlantingCalendarResponseDto>.Error("请选择城市", 400);
        }

        if (!PlantingCalendarData.CityClimateData.TryGetValue(request.City, out var cityClimate))
        {
            return ApiResponse<PlantingCalendarResponseDto>.Error($"暂不支持城市：{request.City}", 404);
        }

        var currentMonth = request.Month ?? DateTime.Now.Month;
        var climateZone = PlantingCalendarData.GetClimateZone(cityClimate.ClimateZone);

        var recommendations = PlantingCalendarData.CropPlantingData
            .Where(crop =>
            {
                if (crop.SuitableMonthsByZone.TryGetValue(climateZone, out var months))
                {
                    return months.Contains(currentMonth);
                }
                return false;
            })
            .Select(crop => new PlantingRecommendationDto
            {
                CropName = crop.Name,
                Variety = crop.Variety,
                PlantingSeason = PlantingCalendarData.GetPlantingSeason(currentMonth, climateZone),
                Difficulty = crop.Difficulty,
                GrowthDays = crop.GrowthDays,
                Tips = crop.Tips,
                SuitableLocation = crop.SuitableLocation
            })
            .ToList();

        var solarTerm = PlantingCalendarData.SolarTermData.TryGetValue(currentMonth, out var termInfo)
            ? termInfo.Term
            : string.Empty;

        var avgTemp = cityClimate.AverageTempByMonth.Length >= currentMonth
            ? cityClimate.AverageTempByMonth[currentMonth - 1]
            : 0;
        var avgPrecipitation = cityClimate.PrecipitationByMonth.Length >= currentMonth
            ? cityClimate.PrecipitationByMonth[currentMonth - 1]
            : 0;

        var climateDescription = $"本月平均气温约 {avgTemp}°C，平均降水量约 {avgPrecipitation}mm。{termInfo?.Description ?? string.Empty}";

        var response = new PlantingCalendarResponseDto
        {
            City = cityClimate.Name,
            Province = cityClimate.Province,
            ClimateZone = cityClimate.ClimateZone,
            Month = currentMonth,
            SolarTerm = solarTerm,
            ClimateDescription = climateDescription,
            Recommendations = recommendations
        };

        return await Task.FromResult(ApiResponse<PlantingCalendarResponseDto>.Success(response));
    }

    public async Task<ApiResponse<CropDto>> CreateCropFromRecommendationAsync(CreateCropFromRecommendationRequestDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从种植日历推荐创建作物: {CropName}, 用户: {UserId}", request.CropName, userId);

        if (string.IsNullOrWhiteSpace(request.CropName))
        {
            return ApiResponse<CropDto>.Error("作物名称不能为空", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return ApiResponse<CropDto>.Error("请填写阳台位置", 400);
        }

        if (string.IsNullOrWhiteSpace(request.ContainerType))
        {
            return ApiResponse<CropDto>.Error("请填写容器类型", 400);
        }

        var crop = new Crop
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.CropName,
            Variety = request.Variety,
            PlantingDate = request.PlantingDate ?? DateTime.UtcNow,
            Location = request.Location,
            ContainerType = request.ContainerType,
            Status = CropStatus.Growing,
            PhotoUrl = request.PhotoUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Crops.AddAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("从种植日历创建作物成功: {CropId}", crop.Id);

        var cropDto = crop.Adapt<CropDto>();
        return ApiResponse<CropDto>.Success(cropDto, "创建成功");
    }
}
