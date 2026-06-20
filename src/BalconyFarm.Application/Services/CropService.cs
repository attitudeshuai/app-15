using BalconyFarm.Application.Data;
using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class CropService : ICropService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CropService> _logger;
    private readonly IPlantingPlanTemplateDataProvider _templateDataProvider;
    private readonly IAchievementService _achievementService;

    public CropService(
        IUnitOfWork unitOfWork,
        ILogger<CropService> logger,
        IPlantingPlanTemplateDataProvider templateDataProvider,
        IAchievementService achievementService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _templateDataProvider = templateDataProvider;
        _achievementService = achievementService;
    }

    public async Task<ApiResponse<PagedResult<CropDto>>> GetCropsAsync(CropQueryRequestDto query, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var cropsList = (await _unitOfWork.Crops.GetAllAsync(cancellationToken)).ToList();

        IEnumerable<Crop> crops = cropsList;

        if (userId.HasValue)
        {
            crops = crops.Where(c => c.UserId == userId.Value);
        }

        if (query.Status.HasValue)
        {
            crops = crops.Where(c => c.Status == query.Status.Value);
        }

        if (!string.IsNullOrEmpty(query.Location))
        {
            crops = crops.Where(c => c.Location.Contains(query.Location));
        }

        if (!string.IsNullOrEmpty(query.ContainerType))
        {
            crops = crops.Where(c => c.ContainerType.Contains(query.ContainerType));
        }

        if (query.PlantingDateFrom.HasValue)
        {
            crops = crops.Where(c => c.PlantingDate >= query.PlantingDateFrom.Value);
        }

        if (query.PlantingDateTo.HasValue)
        {
            crops = crops.Where(c => c.PlantingDate <= query.PlantingDateTo.Value);
        }

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            crops = crops.Where(c =>
                c.Name.Contains(query.SearchKeyword) ||
                c.Variety.Contains(query.SearchKeyword));
        }

        var totalCount = crops.Count();
        var sortFunc = GetSortProperty(query.SortBy ?? "createdat").Compile();

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            crops = query.SortOrder?.ToLower() == "desc"
                ? crops.OrderByDescending(sortFunc)
                : crops.OrderBy(sortFunc);
        }
        else
        {
            crops = crops.OrderByDescending(c => c.CreatedAt);
        }

        var items = crops
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Adapt<IEnumerable<CropDto>>()
            .ToList();

        var result = new PagedResult<CropDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PagedResult<CropDto>>.Success(result);
    }

    public async Task<ApiResponse<CropDto>> GetCropByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var crop = await _unitOfWork.Crops.GetByIdAsync(id, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropDto>.Error("作物不存在", 404);
        }

        if (userId.HasValue && crop.UserId != userId.Value)
        {
            return ApiResponse<CropDto>.Error("无权访问此作物", 403);
        }

        var cropDto = crop.Adapt<CropDto>();
        return ApiResponse<CropDto>.Success(cropDto);
    }

    public async Task<ApiResponse<CropDto>> CreateCropAsync(CreateCropRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建作物: {Name}, 用户: {UserId}", dto.Name, userId);

        var crop = dto.Adapt<Crop>();
        crop.Id = Guid.NewGuid();
        crop.UserId = userId;
        crop.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Crops.AddAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物创建成功: {CropId}", crop.Id);

        await _achievementService.CheckAndUnlockPlantingAchievementsAsync(userId, cancellationToken);

        var cropDto = crop.Adapt<CropDto>();
        return ApiResponse<CropDto>.Success(cropDto, "创建成功");
    }

    public async Task<ApiResponse<CreateCropWithTemplateResultDto>> CreateCropWithTemplateAsync(
        CreateCropRequestDto dto,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建作物并应用模板: {Name}, TemplateId={TemplateId}, 用户: {UserId}",
            dto.Name, dto.PlantingPlanTemplateId, userId);

        var crop = dto.Adapt<Crop>();
        crop.Id = Guid.NewGuid();
        crop.UserId = userId;
        crop.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Crops.AddAsync(crop, cancellationToken);

        var result = new CreateCropWithTemplateResultDto
        {
            Crop = crop.Adapt<CropDto>()
        };

        if (dto.AutoGenerateTasksFromTemplate && !string.IsNullOrEmpty(dto.PlantingPlanTemplateId))
        {
            var template = await _templateDataProvider.GetTemplateByIdAsync(dto.PlantingPlanTemplateId, cancellationToken);
            if (template == null)
            {
                return ApiResponse<CreateCropWithTemplateResultDto>.Error("种植计划模板不存在", 404);
            }

            result.AppliedTemplateId = template.Id;
            result.AppliedTemplateName = $"{template.CropName} - {template.Variety}";

            var allTemplateTasks = template.Stages
                .SelectMany(s => s.Tasks)
                .Concat(template.Tasks)
                .DistinctBy(t => new { t.TaskType, t.DaysAfterPlanting })
                .ToList();

            var generatedTasks = new List<CropCareTaskDto>();

            foreach (var templateTask in allTemplateTasks)
            {
                var scheduledDate = crop.PlantingDate.Date.AddDays(templateTask.DaysAfterPlanting);

                var task = new CropCareTask
                {
                    Id = Guid.NewGuid(),
                    CropId = crop.Id,
                    TaskType = templateTask.TaskType,
                    ScheduledDate = scheduledDate,
                    Status = TaskStatus.Pending,
                    Note = templateTask.DefaultNote
                };

                await _unitOfWork.CropCareTasks.AddAsync(task, cancellationToken);
                generatedTasks.Add(task.Adapt<CropCareTaskDto>());
            }

            result.GeneratedTaskCount = generatedTasks.Count;
            result.GeneratedTasks = generatedTasks;
            _logger.LogInformation("模板应用成功，生成 {TaskCount} 个养护任务", generatedTasks.Count);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        result.Crop = crop.Adapt<CropDto>();
        result.GeneratedTasks.ForEach(t => t.CropName = crop.Name);

        await _achievementService.CheckAndUnlockPlantingAchievementsAsync(userId, cancellationToken);

        var message = result.GeneratedTaskCount > 0
            ? $"创建成功，已根据模板自动生成 {result.GeneratedTaskCount} 个养护任务"
            : "创建成功";

        return ApiResponse<CreateCropWithTemplateResultDto>.Success(result, message);
    }

    public async Task<ApiResponse<CropDto>> UpdateCropAsync(Guid id, UpdateCropRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新作物: {CropId}, 用户: {UserId}", id, userId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(id, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropDto>.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse<CropDto>.Error("无权修改此作物", 403);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            crop.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Variety))
            crop.Variety = dto.Variety;
        if (dto.PlantingDate.HasValue)
            crop.PlantingDate = dto.PlantingDate.Value;
        if (!string.IsNullOrEmpty(dto.Location))
            crop.Location = dto.Location;
        if (!string.IsNullOrEmpty(dto.ContainerType))
            crop.ContainerType = dto.ContainerType;
        if (dto.Status.HasValue)
            crop.Status = dto.Status.Value;
        if (dto.PhotoUrl != null)
            crop.PhotoUrl = dto.PhotoUrl;

        await _unitOfWork.Crops.UpdateAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物更新成功: {CropId}", id);

        var cropDto = crop.Adapt<CropDto>();
        return ApiResponse<CropDto>.Success(cropDto, "更新成功");
    }

    public async Task<ApiResponse> DeleteCropAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除作物: {CropId}, 用户: {UserId}", id, userId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(id, cancellationToken);
        if (crop == null)
        {
            return ApiResponse.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse.Error("无权删除此作物", 403);
        }

        await _unitOfWork.Crops.DeleteAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物删除成功: {CropId}", id);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<CropDto>> UpdateCropStatusAsync(Guid id, UpdateCropStatusRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新作物状态: {CropId}, 状态: {Status}, 用户: {UserId}", id, dto.Status, userId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(id, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropDto>.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse<CropDto>.Error("无权修改此作物", 403);
        }

        crop.Status = dto.Status;

        await _unitOfWork.Crops.UpdateAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("作物状态更新成功: {CropId}", id);

        var cropDto = crop.Adapt<CropDto>();
        return ApiResponse<CropDto>.Success(cropDto, "状态更新成功");
    }

    public async Task<ApiResponse<PagedResult<CropDto>>> GetMyCropsAsync(CropQueryRequestDto query, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetCropsAsync(query, userId, cancellationToken);
    }

    public async Task<ApiResponse<CropShareCardDto>> GetCropShareCardAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var crop = await _unitOfWork.Crops.GetByIdAsync(id, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<CropShareCardDto>.Error("作物不存在", 404);
        }

        if (userId.HasValue && crop.UserId != userId.Value)
        {
            return ApiResponse<CropShareCardDto>.Error("无权访问此作物", 403);
        }

        var harvestRecords = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => h.CropId == id)
            .OrderByDescending(h => h.HarvestDate)
            .ToList();

        var growthDays = (DateTime.UtcNow - crop.PlantingDate).Days;

        string? sharePhotoUrl = crop.PhotoUrl;
        if (sharePhotoUrl == null && harvestRecords.Count > 0)
        {
            sharePhotoUrl = harvestRecords.FirstOrDefault(h => !string.IsNullOrEmpty(h.PhotoUrl))?.PhotoUrl;
        }

        var harvestSummary = new HarvestSummaryDto
        {
            TotalHarvestCount = harvestRecords.Count,
            TotalQuantity = harvestRecords.Sum(h => h.Quantity),
            Unit = harvestRecords.FirstOrDefault()?.Unit ?? string.Empty,
            LatestQualityNote = harvestRecords.FirstOrDefault()?.QualityNote
        };

        var shareCard = new CropShareCardDto
        {
            CropId = crop.Id,
            CropName = crop.Name,
            Variety = crop.Variety,
            GrowthDays = growthDays,
            Status = crop.Status.ToString(),
            PhotoUrl = sharePhotoUrl,
            OwnerUsername = crop.User?.Username,
            HarvestSummary = harvestSummary
        };

        return ApiResponse<CropShareCardDto>.Success(shareCard);
    }

    private static System.Linq.Expressions.Expression<Func<Crop, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "name" => crop => crop.Name,
            "plantingdate" => crop => crop.PlantingDate,
            "status" => crop => crop.Status,
            "createdat" => crop => crop.CreatedAt,
            _ => crop => crop.CreatedAt
        };
    }
}
