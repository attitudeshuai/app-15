using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;
using TaskStatus = BalconyFarm.Domain.Enums.TaskStatus;

namespace BalconyFarm.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(NotificationQueryRequestDto query, Guid userId, CancellationToken cancellationToken = default)
    {
        var allNotifications = (await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId, cancellationToken))
            .ToList();

        IEnumerable<Notification> filtered = allNotifications;

        if (query.IsRead.HasValue)
        {
            filtered = filtered.Where(n => n.IsRead == query.IsRead.Value);
        }

        if (query.NotificationType.HasValue)
        {
            filtered = filtered.Where(n => n.NotificationType == query.NotificationType.Value);
        }

        var totalCount = filtered.Count();

        filtered = filtered.OrderByDescending(n => n.CreatedAt);

        var items = filtered
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var cropIds = items.Select(n => n.CropCareTaskId).Distinct().ToList();
        var taskDict = new Dictionary<Guid, CropCareTask>();
        var cropDict = new Dictionary<Guid, string>();

        foreach (var taskId in cropIds)
        {
            var task = await _unitOfWork.CropCareTasks.GetByIdAsync(taskId, cancellationToken);
            if (task != null)
            {
                taskDict[taskId] = task;
                if (!cropDict.ContainsKey(task.CropId))
                {
                    var crop = await _unitOfWork.Crops.GetByIdAsync(task.CropId, cancellationToken);
                    if (crop != null)
                    {
                        cropDict[task.CropId] = crop.Name;
                    }
                }
            }
        }

        var dtoItems = items.Select(n =>
        {
            var dto = n.Adapt<NotificationDto>();
            if (taskDict.TryGetValue(n.CropCareTaskId, out var task))
            {
                var taskDto = task.Adapt<CropCareTaskDto>();
                if (cropDict.TryGetValue(task.CropId, out var cropName))
                {
                    taskDto.CropName = cropName;
                }
                SetOverdueInfo(taskDto);
                dto.Task = taskDto;
            }
            return dto;
        }).ToList();

        var result = new PagedResult<NotificationDto>
        {
            Items = dtoItems,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PagedResult<NotificationDto>>.Success(result);
    }

    public async Task<ApiResponse<UnreadCountDto>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
        return ApiResponse<UnreadCountDto>.Success(new UnreadCountDto { Count = count });
    }

    public async Task<ApiResponse<NotificationDto>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null)
        {
            return ApiResponse<NotificationDto>.Error("通知不存在", 404);
        }

        if (notification.UserId != userId)
        {
            return ApiResponse<NotificationDto>.Error("无权操作此通知", 403);
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var dto = notification.Adapt<NotificationDto>();
        return ApiResponse<NotificationDto>.Success(dto, "标记已读成功");
    }

    public async Task<ApiResponse> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = (await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead, cancellationToken)).ToList();

        if (!unreadNotifications.Any())
        {
            return ApiResponse.Success(null, "没有未读通知");
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("用户 {UserId} 标记所有通知为已读，共 {Count} 条", userId, unreadNotifications.Count);

        return ApiResponse.Success(null, $"已标记 {unreadNotifications.Count} 条通知为已读");
    }

    public async Task GenerateRemindersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("开始生成养护任务提醒通知");

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var pendingTasks = (await _unitOfWork.CropCareTasks.FindAsync(
            t => t.Status == TaskStatus.Pending || t.Status == TaskStatus.InProgress,
            cancellationToken)).ToList();

        if (!pendingTasks.Any())
        {
            _logger.LogInformation("没有待处理的养护任务，跳过提醒生成");
            return;
        }

        var cropIds = pendingTasks.Select(t => t.CropId).Distinct().ToList();
        var cropDict = new Dictionary<Guid, Crop>();
        foreach (var cropId in cropIds)
        {
            var crop = await _unitOfWork.Crops.GetByIdAsync(cropId, cancellationToken);
            if (crop != null)
            {
                cropDict[cropId] = crop;
            }
        }

        var existingNotifications = (await _unitOfWork.Notifications.GetAllAsync(cancellationToken)).ToList();

        var dayBeforeExists = existingNotifications
            .Where(n => n.NotificationType == NotificationType.DayBeforeReminder)
            .Select(n => n.CropCareTaskId)
            .ToHashSet();

        var sameDayExists = existingNotifications
            .Where(n => n.NotificationType == NotificationType.SameDayReminder)
            .Select(n => n.CropCareTaskId)
            .ToHashSet();

        var newNotifications = new List<Notification>();

        foreach (var task in pendingTasks)
        {
            if (!cropDict.TryGetValue(task.CropId, out var crop))
            {
                continue;
            }

            var scheduledDate = task.ScheduledDate.Date;
            var taskTypeName = GetTaskTypeName(task.TaskType);

            if (scheduledDate == tomorrow && !dayBeforeExists.Contains(task.Id))
            {
                newNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = crop.UserId,
                    CropCareTaskId = task.Id,
                    Title = $"明日养护提醒：{crop.Name}",
                    Message = $"明天（{tomorrow:yyyy-MM-dd}）需要为「{crop.Name}」执行{taskTypeName}任务，请提前准备。",
                    NotificationType = NotificationType.DayBeforeReminder,
                    IsRead = false
                });
            }

            if (scheduledDate == today && !sameDayExists.Contains(task.Id))
            {
                newNotifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = crop.UserId,
                    CropCareTaskId = task.Id,
                    Title = $"今日养护提醒：{crop.Name}",
                    Message = $"今天（{today:yyyy-MM-dd}）需要为「{crop.Name}」执行{taskTypeName}任务，请尽快完成。",
                    NotificationType = NotificationType.SameDayReminder,
                    IsRead = false
                });
            }
        }

        foreach (var notification in newNotifications)
        {
            await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        }

        if (newNotifications.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("提醒通知生成完成，新增 {Count} 条通知", newNotifications.Count);
    }

    private static string GetTaskTypeName(TaskType taskType)
    {
        return taskType switch
        {
            TaskType.Water => "浇水",
            TaskType.Fertilize => "施肥",
            TaskType.Prune => "修剪",
            TaskType.Repot => "换盆",
            _ => "养护"
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
