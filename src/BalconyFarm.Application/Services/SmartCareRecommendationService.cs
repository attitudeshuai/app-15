using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Data;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class SmartCareRecommendationService : ISmartCareRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICareRuleDataProvider _careRuleDataProvider;
    private readonly ILogger<SmartCareRecommendationService> _logger;

    private static readonly Dictionary<GrowthStage, string> GrowthStageNames = new()
    {
        { GrowthStage.Seedling, "幼苗期" },
        { GrowthStage.Vegetative, "营养生长期" },
        { GrowthStage.Flowering, "开花期" },
        { GrowthStage.Fruiting, "结果期" },
        { GrowthStage.Mature, "成熟期" }
    };

    public SmartCareRecommendationService(
        IUnitOfWork unitOfWork,
        ICareRuleDataProvider careRuleDataProvider,
        ILogger<SmartCareRecommendationService> logger)
    {
        _unitOfWork = unitOfWork;
        _careRuleDataProvider = careRuleDataProvider;
        _logger = logger;
    }

    public async Task<ApiResponse<GenerateCareTasksResultDto>> PreviewCareTasksAsync(GenerateCareTasksRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GenerateOrPreviewAsync(dto, userId, false, cancellationToken);
    }

    public async Task<ApiResponse<GenerateCareTasksResultDto>> GenerateCareTasksAsync(GenerateCareTasksRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GenerateOrPreviewAsync(dto, userId, true, cancellationToken);
    }

    private async Task<ApiResponse<GenerateCareTasksResultDto>> GenerateOrPreviewAsync(
        GenerateCareTasksRequestDto dto,
        Guid userId,
        bool createTasks,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("智能养护任务推荐: CropId={CropId}, DaysAhead={DaysAhead}, Create={Create}, User={UserId}",
            dto.CropId, dto.DaysAhead, createTasks, userId);

        if (dto.DaysAhead <= 0 || dto.DaysAhead > 365)
        {
            return ApiResponse<GenerateCareTasksResultDto>.Error("预测天数必须在1-365天之间", 400);
        }

        var crop = await _unitOfWork.Crops.GetByIdAsync(dto.CropId, cancellationToken);
        if (crop == null)
        {
            return ApiResponse<GenerateCareTasksResultDto>.Error("作物不存在", 404);
        }

        if (crop.UserId != userId)
        {
            return ApiResponse<GenerateCareTasksResultDto>.Error("无权为此作物生成任务", 403);
        }

        var careRule = await _careRuleDataProvider.GetCareRuleByCropNameAsync(crop.Name, cancellationToken);
        if (careRule == null && !string.IsNullOrEmpty(crop.Variety))
        {
            careRule = await _careRuleDataProvider.GetCareRuleByCropNameAsync(crop.Variety, cancellationToken);
        }

        if (careRule == null)
        {
            _logger.LogWarning("未找到作物 {CropName}({Variety}) 的养护规则，使用默认规则", crop.Name, crop.Variety);
            careRule = GetDefaultCareRule(crop);
        }

        var daysSincePlanting = (int)(DateTime.UtcNow.Date - crop.PlantingDate.Date).TotalDays;
        if (daysSincePlanting < 0) daysSincePlanting = 0;

        var currentStage = GetCurrentGrowthStage(daysSincePlanting, careRule);
        var result = new GenerateCareTasksResultDto
        {
            CropId = crop.Id,
            CropName = crop.Name,
            CurrentGrowthStage = currentStage,
            CurrentGrowthStageName = GrowthStageNames.GetValueOrDefault(currentStage, "未知"),
            DaysSincePlanting = daysSincePlanting,
            TotalGrowthDays = careRule.TotalGrowthDays
        };

        var existingTasks = (await _unitOfWork.CropCareTasks.FindAsync(
            t => t.CropId == crop.Id, cancellationToken)).ToList();

        var generatedCount = 0;
        var skippedCount = 0;
        var createdTasks = new List<CropCareTask>();

        for (var offsetDay = 0; offsetDay < dto.DaysAhead; offsetDay++)
        {
            var taskDay = daysSincePlanting + offsetDay;
            var scheduledDate = DateTime.UtcNow.Date.AddDays(offsetDay);
            var stageForDay = GetCurrentGrowthStage(taskDay, careRule);
            var stageRule = careRule.StageRules.FirstOrDefault(s => s.Stage == stageForDay);

            if (stageRule == null) continue;

            foreach (var taskRule in stageRule.Tasks)
            {
                var dayInStage = taskDay - stageRule.StartDay;
                if (dayInStage < 0) continue;

                if (dayInStage % taskRule.IntervalDays != 0) continue;

                var recommendedTask = new RecommendedTaskDto
                {
                    TaskType = taskRule.TaskType,
                    ScheduledDate = scheduledDate,
                    Note = taskRule.DefaultNote,
                    GrowthStage = stageForDay,
                    GrowthStageName = GrowthStageNames.GetValueOrDefault(stageForDay, "未知")
                };
                result.RecommendedTasks.Add(recommendedTask);

                if (!createTasks) continue;

                var hasConflict = existingTasks.Any(t =>
                    t.TaskType == taskRule.TaskType &&
                    t.ScheduledDate.Date == scheduledDate.Date &&
                    t.Status != TaskStatus.Cancelled);

                if (hasConflict && !dto.OverwriteExisting)
                {
                    skippedCount++;
                    continue;
                }

                if (hasConflict && dto.OverwriteExisting)
                {
                    var existing = existingTasks.First(t =>
                        t.TaskType == taskRule.TaskType &&
                        t.ScheduledDate.Date == scheduledDate.Date);
                    existing.Note = taskRule.DefaultNote;
                    await _unitOfWork.CropCareTasks.UpdateAsync(existing, cancellationToken);
                    createdTasks.Add(existing);
                }
                else
                {
                    var newTask = new CropCareTask
                    {
                        Id = Guid.NewGuid(),
                        CropId = crop.Id,
                        TaskType = taskRule.TaskType,
                        ScheduledDate = scheduledDate,
                        Status = TaskStatus.Pending,
                        Note = taskRule.DefaultNote
                    };
                    await _unitOfWork.CropCareTasks.AddAsync(newTask, cancellationToken);
                    createdTasks.Add(newTask);
                    existingTasks.Add(newTask);
                }
                generatedCount++;
            }
        }

        if (createTasks)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            result.CreatedTasks = createdTasks.Adapt<List<CropCareTaskDto>>();
            foreach (var t in result.CreatedTasks)
            {
                t.CropName = crop.Name;
                SetOverdueInfo(t);
            }
        }

        result.GeneratedCount = generatedCount;
        result.SkippedCount = skippedCount;

        _logger.LogInformation("智能养护任务推荐完成: 推荐{Recommended}条, 创建{Created}条, 跳过{Skipped}条",
            result.RecommendedTasks.Count, generatedCount, skippedCount);

        return ApiResponse<GenerateCareTasksResultDto>.Success(result, createTasks ? "任务生成成功" : "任务预览成功");
    }

    private static GrowthStage GetCurrentGrowthStage(int daysSincePlanting, CropCareRule rule)
    {
        foreach (var stage in rule.StageRules.OrderBy(s => s.StartDay))
        {
            if (daysSincePlanting >= stage.StartDay && daysSincePlanting <= stage.EndDay)
            {
                return stage.Stage;
            }
        }
        return rule.StageRules.OrderBy(s => s.StartDay).Last().Stage;
    }

    private static CropCareRule GetDefaultCareRule(Crop crop)
    {
        return new CropCareRule
        {
            CropName = crop.Name,
            TotalGrowthDays = 90,
            StageRules = new List<GrowthStageCareRule>
            {
                new()
                {
                    Stage = GrowthStage.Seedling,
                    StartDay = 0,
                    EndDay = 14,
                    Tasks = new List<CareTaskRule>
                    {
                        new() { TaskType = TaskType.Water, IntervalDays = 2, DefaultNote = "幼苗期保持土壤微湿" },
                        new() { TaskType = TaskType.Fertilize, IntervalDays = 14, DefaultNote = "幼苗期施用稀薄氮肥" }
                    }
                },
                new()
                {
                    Stage = GrowthStage.Vegetative,
                    StartDay = 15,
                    EndDay = 45,
                    Tasks = new List<CareTaskRule>
                    {
                        new() { TaskType = TaskType.Water, IntervalDays = 1, DefaultNote = "生长期保持土壤湿润" },
                        new() { TaskType = TaskType.Fertilize, IntervalDays = 7, DefaultNote = "生长期施用均衡肥料" }
                    }
                },
                new()
                {
                    Stage = GrowthStage.Mature,
                    StartDay = 46,
                    EndDay = 90,
                    Tasks = new List<CareTaskRule>
                    {
                        new() { TaskType = TaskType.Water, IntervalDays = 2, DefaultNote = "成熟期适当控水" },
                        new() { TaskType = TaskType.Fertilize, IntervalDays = 10, DefaultNote = "成熟期减少施肥" }
                    }
                }
            }
        };
    }

    private static void SetOverdueInfo(CropCareTaskDto dto)
    {
        var isPending = dto.Status == TaskStatus.Pending || dto.Status == TaskStatus.InProgress;
        if (isPending && dto.ScheduledDate.Date < DateTime.UtcNow.Date)
        {
            dto.IsOverdue = true;
            dto.OverdueDays = (int)(DateTime.UtcNow.Date - dto.ScheduledDate.Date).TotalDays;
        }
        else
        {
            dto.IsOverdue = false;
            dto.OverdueDays = null;
        }
    }
}
