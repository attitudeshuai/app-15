namespace BalconyFarm.Domain.Entities;

public class PlantingLocation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LocationType { get; set; }
    public string? SunlightCondition { get; set; }
    public decimal? Area { get; set; }
    public string? PhotoUrl { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Crop> Crops { get; set; } = new List<Crop>();
}
