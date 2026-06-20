using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(NotificationQueryRequestDto query, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UnreadCountDto>> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<NotificationDto>> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task GenerateRemindersAsync(CancellationToken cancellationToken = default);
}
