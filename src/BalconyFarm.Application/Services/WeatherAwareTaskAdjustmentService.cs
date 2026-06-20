using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class WeatherAwareTaskAdjustmentService : IWeatherAwareTaskAdjustmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWeatherForecastService _weatherForecastService;
    private readonly ILogger<WeatherAwareTaskAdjustmentService> _logger;

    public WeatherAwareTaskAdjustmentService(
        IUnitOfWork unitOfWork,
        IWeatherForecastService weatherForecastService,
        ILogger<WeatherAwareTaskAdjustmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _weatherForecastService = weatherForecastService;
        _logger = logger;
    }

    public async Task<ApiResponse<WeatherAdjustTaskResultDto>> AdjustUpcomingWateringTasksAsync(
        WeatherAdjustTaskRequestDto dto,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("天气智能调整浇水任务: User={UserId}, CropId={CropId}, DaysAhead={DaysAhead}, DryRun={DryRun}",
            userId, dto.CropId, dto.DaysAhead, dto.DryRun);

        if (dto.DaysAhead <= 0 || dto.DaysAhead > 30)
        {
            return ApiResponse<WeatherAdjustTaskResultDto>.Error("预测天数必须在1-30天之间", 400);
        }

        var result = new WeatherAdjustTaskResultDto();

        var userCrops = (await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken)).ToList();
        if (userCrops.Count == 0)
        {
            return ApiResponse<WeatherAdjustTaskResultDto>.Success(result, "用户没有作物");
        }

        var userCropIds = userCrops.Select(c => c.Id).ToHashSet();

        if (dto.CropId.HasValue && !userCropIds.Contains(dto.CropId.Value))
        {
            return ApiResponse<WeatherAdjustTaskResultDto>.Error("无权访问此作物", 403);
        }

        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(dto.DaysAhead);

        var allTasks = await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken);
        var pendingWaterTasks = allTasks
            .Where(t =>
                t.TaskType == TaskType.Water &&
                t.Status == TaskStatus.Pending &&
                t.ScheduledDate.Date >= today &&
                t.ScheduledDate.Date <= endDate &&
                userCropIds.Contains(t.CropId) &&
                (!dto.CropId.HasValue || t.CropId == dto.CropId.Value))
            .OrderBy(t => t.ScheduledDate)
            .ToList();

        result.TotalTasksChecked = pendingWaterTasks.Count;

        var cropDict = userCrops.ToDictionary(c => c.Id, c => c);

        foreach (var task in pendingWaterTasks)
        {
            try
            {
                if (!cropDict.TryGetValue(task.CropId, out var crop))
                {
                    result.Failures.Add(new WeatherAdjustTaskFailureDto
                    {
                        TaskId = task.Id,
                        ErrorMessage = "关联作物不存在"
                    });
                    result.TasksFailed++;
                    continue;
                }

                var assessment = await _weatherForecastService.AssessWateringNeedAsync(
                    crop.Location, task.ScheduledDate, cancellationToken);

                var adjustedTask = new WeatherAdjustedTaskDto
                {
                    TaskId = task.Id,
                    CropId = crop.Id,
                    CropName = crop.Name,
                    OriginalScheduledDate = task.ScheduledDate,
                    AdjustmentReason = assessment.AdjustmentReason,
                    WeatherOnScheduleDate = assessment.WeatherOnScheduledDate
                };

                if (assessment.ShouldSkipWatering)
                {
                    adjustedTask.Action = "取消";
                    result.TasksSkipped++;

                    if (!dto.DryRun)
                    {
                        task.Status = TaskStatus.Cancelled;
                        task.WeatherAdjusted = true;
                        task.WeatherAdjustmentReason = assessment.AdjustmentReason;
                        task.WeatherAdjustedAt = DateTime.UtcNow;
                        task.WeatherCity = crop.Location;
                        task.WeatherTemperatureC = assessment.WeatherOnScheduledDate?.TemperatureC;
                        task.WeatherPrecipitationMm = assessment.WeatherOnScheduledDate?.PrecipitationMm;
                        task.Note = string.IsNullOrEmpty(task.Note)
                            ? $"[天气自动取消] {assessment.AdjustmentReason}"
                            : $"[天气自动取消] {assessment.AdjustmentReason} | {task.Note}";
                        await _unitOfWork.CropCareTasks.UpdateAsync(task, cancellationToken);
                    }

                    result.AdjustedTasks.Add(adjustedTask);
                }
                else if (assessment.ShouldDelayWatering && assessment.DelayDays > 0)
                {
                    var newDate = task.ScheduledDate.AddDays(assessment.DelayDays.Value);
                    adjustedTask.NewScheduledDate = newDate;
                    adjustedTask.Action = $"延后{assessment.DelayDays}天";
                    result.TasksDelayed++;

                    if (!dto.DryRun)
                    {
                        task.ScheduledDate = newDate;
                        task.WeatherAdjusted = true;
                        task.WeatherAdjustmentReason = assessment.AdjustmentReason;
                        task.WeatherAdjustedAt = DateTime.UtcNow;
                        task.WeatherCity = crop.Location;
                        task.WeatherTemperatureC = assessment.WeatherOnScheduledDate?.TemperatureC;
                        task.WeatherPrecipitationMm = assessment.WeatherOnScheduledDate?.PrecipitationMm;
                        task.Note = string.IsNullOrEmpty(task.Note)
                            ? $"[天气自动调整] {assessment.AdjustmentReason}"
                            : $"[天气自动调整] {assessment.AdjustmentReason} | {task.Note}";
                        await _unitOfWork.CropCareTasks.UpdateAsync(task, cancellationToken);
                    }

                    result.AdjustedTasks.Add(adjustedTask);
                }
                else if (assessment.ShouldDelayWatering)
                {
                    adjustedTask.Action = "提醒";
                    result.TasksDelayed++;

                    if (!dto.DryRun)
                    {
                        task.WeatherAdjusted = true;
                        task.WeatherAdjustmentReason = assessment.AdjustmentReason;
                        task.WeatherAdjustedAt = DateTime.UtcNow;
                        task.WeatherCity = crop.Location;
                        task.WeatherTemperatureC = assessment.WeatherOnScheduledDate?.TemperatureC;
                        task.WeatherPrecipitationMm = assessment.WeatherOnScheduledDate?.PrecipitationMm;
                        task.Note = string.IsNullOrEmpty(task.Note)
                            ? $"[天气提醒] {assessment.AdjustmentReason}"
                            : $"[天气提醒] {assessment.AdjustmentReason} | {task.Note}";
                        await _unitOfWork.CropCareTasks.UpdateAsync(task, cancellationToken);
                    }

                    result.AdjustedTasks.Add(adjustedTask);
                }
                else
                {
                    adjustedTask.Action = "无需调整";
                    result.TasksUnchanged++;

                    if (!dto.DryRun)
                    {
                        task.WeatherAdjusted = true;
                        task.WeatherAdjustmentReason = assessment.AdjustmentReason;
                        task.WeatherAdjustedAt = DateTime.UtcNow;
                        task.WeatherCity = crop.Location;
                        task.WeatherTemperatureC = assessment.WeatherOnScheduledDate?.TemperatureC;
                        task.WeatherPrecipitationMm = assessment.WeatherOnScheduledDate?.PrecipitationMm;
                        await _unitOfWork.CropCareTasks.UpdateAsync(task, cancellationToken);
                    }

                    result.AdjustedTasks.Add(adjustedTask);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理天气调整任务失败: TaskId={TaskId}", task.Id);
                result.Failures.Add(new WeatherAdjustTaskFailureDto
                {
                    TaskId = task.Id,
                    ErrorMessage = $"处理失败: {ex.Message}"
                });
                result.TasksFailed++;
            }
        }

        if (!dto.DryRun)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("天气智能调整完成: 检查{Total}条, 取消{Skipped}条, 延后{Delayed}条, 不变{Unchanged}条, 失败{Failed}条",
            result.TotalTasksChecked, result.TasksSkipped, result.TasksDelayed, result.TasksUnchanged, result.TasksFailed);

        var action = dto.DryRun ? "预览" : "执行";
        var message = $"天气调整{action}完成：检查{result.TotalTasksChecked}条任务，取消{result.TasksSkipped}条，延后{result.TasksDelayed}条，无需调整{result.TasksUnchanged}条" +
                      (result.TasksFailed > 0 ? $"，失败{result.TasksFailed}条" : "");

        return ApiResponse<WeatherAdjustTaskResultDto>.Success(result, message);
    }
}
