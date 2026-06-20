namespace BalconyFarm.Domain.Entities;

public class CropPhoto
{
    public Guid Id { get; set; }
    public Guid CropId { get; set; }
    public DateTime PhotoDate { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DaysAfterPlanting { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Crop? Crop { get; set; }
}
