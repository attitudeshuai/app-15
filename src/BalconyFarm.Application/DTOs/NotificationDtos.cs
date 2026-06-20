using BalconyFarm.Domain.Enums;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CropCareTaskId { get; set; }
    public Guid? SeedInventoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public CropCareTaskDto? Task { get; set; }
    public SeedInventoryDto? SeedInventory { get; set; }
}

public class NotificationQueryRequestDto : PagedRequest
{
    public bool? IsRead { get; set; }
    public NotificationType? NotificationType { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}
