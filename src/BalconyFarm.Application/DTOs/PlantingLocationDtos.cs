namespace BalconyFarm.Application.DTOs;

public class PlantingLocationDto
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CropCount { get; set; }
}

public class CreatePlantingLocationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LocationType { get; set; }
    public string? SunlightCondition { get; set; }
    public decimal? Area { get; set; }
    public string? PhotoUrl { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePlantingLocationRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LocationType { get; set; }
    public string? SunlightCondition { get; set; }
    public decimal? Area { get; set; }
    public string? PhotoUrl { get; set; }
    public int? SortOrder { get; set; }
}

public class PlantingLocationStatsDto
{
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int TotalCrops { get; set; }
    public int GrowingCrops { get; set; }
    public int HarvestingCrops { get; set; }
    public int FinishedCrops { get; set; }
    public int TotalHarvestRecords { get; set; }
    public decimal TotalHarvestQuantity { get; set; }
    public int ActivePestIssues { get; set; }
}
