using BalconyFarm.Domain.Enums;

namespace BalconyFarm.Domain.Entities;

public class Notification
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public CropCareTask? CropCareTask { get; set; }
    public SeedInventory? SeedInventory { get; set; }
}
