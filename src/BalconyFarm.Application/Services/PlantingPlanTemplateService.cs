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

public class PlantingPlanTemplateService : IPlantingPlanTemplateService
{
    private readonly IPlantingPlanTemplateDataProvider _dataProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlantingPlanTemplateService> _logger;

    public PlantingPlanTemplateService(
        IPlantingPlanTemplateDataProvider dataProvider,
        IUnitOfWork unitOfWork,
        ILogger<PlantingPlanTemplateService> logger)
    {
        _dataProvider = dataProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<PlantingPlanTemplateDto>>> GetAllTemplatesAsync(
        PlantingPlanTemplateQueryRequestDto query,
        CancellationToken cancellationToken = default)
    {
        var templates = await _dataProvider.GetAllTemplatesAsync(cancellationToken);
        var filtered = FilterTemplates(templates, query);
        return ApiResponse<PagedResult<PlantingPlanTemplateDto>>.Success(CreatePagedResult(filtered, query));
    }

    public async Task<ApiResponse<PlantingPlanTemplateDto>> GetTemplateByIdAsync(
        string templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _dataProvider.GetTemplateByIdAsync(templateId, cancellationToken);
        if (template == null)
        {
            return ApiResponse<PlantingPlanTemplateDto>.Error("模板不存在", 404);
        }
        return ApiResponse<PlantingPlanTemplateDto>.Success(MapToDto(template));
    }

    public async Task<ApiResponse<PlantingPlanTemplateDto>> GetTemplateByCropNameAsync(
        string cropName,
        CancellationToken cancellationToken = default)
    {
        var template = await _dataProvider.GetTemplateByCropNameAsync(cropName, cancellationToken);
        if (template == null)
        {
            return ApiResponse<PlantingPlanTemplateDto>.Error("未找到对应作物的养护模板", 404);
        }
        return ApiResponse<PlantingPlanTemplateDto>.Success(MapToDto(template));
    }

    public async Task<ApiResponse<PagedResult<PlantingPlanTemplateDto>>> SearchTemplatesAsync(
        PlantingPlanTemplateQueryRequestDto query,
        CancellationToken cancellationToken = default)
    {
        var templates = await _dataProvider.SearchTemplatesAsync(query.Keyword ?? string.Empty, cancellationToken);
        var filtered = FilterTemplates(templates, query);
        return ApiResponse<PagedResult<PlantingPlanTemplateDto>>.Success(CreatePagedResult(filtered, query));
    }

    public async Task<ApiResponse<ApplyTemplateResultDto>> ApplyTemplateAsync(
        ApplyTemplateRequestDto dto,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("应用种植计划模板: TemplateId={TemplateId}, CropId={CropId}, 用户: {UserId}",
            dto.TemplateId, dto.CropId, userId);

        var crop = await _unitOfWork.Crops.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<ApplyTemplateResultDto>.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse<ApplyTemplateResultDto>.Error("无权为此作物应用模板", 403);
        }

        var template = await _dataProvider.GetTemplateByIdAsync(dto.TemplateId, cancellationToken);
        if (template == null)
        {
            return ApiResponse<ApplyTemplateResultDto>.Error("模板不存在", 404);
        }

        var existingTasks = (await _unitOfWork.CropCareTasks.FindAsync(
            t => t.CropId == dto.CropId, cancellationToken)).ToList();

        var result = new ApplyTemplateResultDto
        {
            CropId = dto.CropId,
            CropName = crop.Name,
            TemplateId = template.Id,
            TemplateName = $"{template.CropName} - {template.Variety}"
        };

        var allTemplateTasks = template.Stages.SelectMany(s => s.Tasks).Concat(template.Tasks).DistinctBy(t => new { t.TaskType, t.DaysAfterPlanting }).ToList();
        var plantingDate = dto.PlantingDate != default ? dto.PlantingDate : crop.PlantingDate;
        var createdTaskDtos = new List<CropCareTaskDto>();

        foreach (var templateTask in allTemplateTasks)
        {
            var scheduledDate = plantingDate.Date.AddDays(templateTask.DaysAfterPlanting);

            if (!dto.OverwriteExisting)
            {
                var exists = existingTasks.Any(t =>
                    t.TaskType == templateTask.TaskType &&
                    t.ScheduledDate.Date == scheduledDate);
                if (exists)
                {
                    result.SkippedTaskCount++;
                    continue;
                }
            }

            var task = new CropCareTask
            {
                Id = Guid.NewGuid(),
                CropId = dto.CropId,
                TaskType = templateTask.TaskType,
                ScheduledDate = scheduledDate,
                Status = TaskStatus.Pending,
                Note = templateTask.DefaultNote
            };

            await _unitOfWork.CropCareTasks.AddAsync(task, cancellationToken);
            result.CreatedTaskCount++;
            createdTaskDtos.Add(task.Adapt<CropCareTaskDto>());
        }

        if (dto.OverwriteExisting && existingTasks.Count > 0)
        {
            foreach (var existingTask in existingTasks)
            {
                await _unitOfWork.CropCareTasks.DeleteAsync(existingTask, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        createdTaskDtos.ForEach(t => t.CropName = crop.Name);
        result.CreatedTasks = createdTaskDtos;

        _logger.LogInformation("模板应用成功: 生成 {CreatedCount} 个任务，跳过 {SkippedCount} 个任务",
            result.CreatedTaskCount, result.SkippedTaskCount);

        var message = result.SkippedTaskCount > 0
            ? $"成功生成 {result.CreatedTaskCount} 个任务，跳过 {result.SkippedTaskCount} 个已存在的任务"
            : $"成功生成 {result.CreatedTaskCount} 个养护任务";

        return ApiResponse<ApplyTemplateResultDto>.Success(result, message);
    }

    private static List<PlantingPlanTemplate> FilterTemplates(
        List<PlantingPlanTemplate> templates,
        PlantingPlanTemplateQueryRequestDto query)
    {
        var result = templates.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Difficulty))
        {
            result = result.Where(t => t.Difficulty.Equals(query.Difficulty, StringComparison.OrdinalIgnoreCase));
        }

        return result.ToList();
    }

    private static PagedResult<PlantingPlanTemplateDto> CreatePagedResult(
        List<PlantingPlanTemplate> templates,
        PlantingPlanTemplateQueryRequestDto query)
    {
        var totalCount = templates.Count;
        var sortBy = query.SortBy ?? "cropname";
        var sortOrder = query.SortOrder ?? "asc";

        IEnumerable<PlantingPlanTemplate> sorted = sortBy.ToLower() switch
        {
            "difficulty" => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? templates.OrderByDescending(t => t.Difficulty)
                : templates.OrderBy(t => t.Difficulty),
            "totalgrowthdays" => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? templates.OrderByDescending(t => t.TotalGrowthDays)
                : templates.OrderBy(t => t.TotalGrowthDays),
            _ => sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? templates.OrderByDescending(t => t.CropName)
                : templates.OrderBy(t => t.CropName)
        };

        var items = sorted
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();

        return new PagedResult<PlantingPlanTemplateDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    private static PlantingPlanTemplateDto MapToDto(PlantingPlanTemplate template)
    {
        var dto = template.Adapt<PlantingPlanTemplateDto>();
        dto.TotalTasks = template.Stages.Sum(s => s.Tasks.Count) + template.Tasks.Count;
        dto.Stages = template.Stages.Select(s => new PlantingPlanTemplateStageDto
        {
            Stage = s.Stage,
            StageName = s.StageName,
            StartDay = s.StartDay,
            EndDay = s.EndDay,
            Description = s.Description,
            Tasks = s.Tasks.Select(t => new PlantingPlanTemplateTaskDto
            {
                TaskType = t.TaskType,
                DaysAfterPlanting = t.DaysAfterPlanting,
                DefaultNote = t.DefaultNote,
                GrowthStage = t.GrowthStage
            }).ToList()
        }).ToList();
        return dto;
    }
}
