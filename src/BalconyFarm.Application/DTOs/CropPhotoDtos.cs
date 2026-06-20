using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class CropPhotoDto
{
    public Guid Id { get; set; }
    public Guid CropId { get; set; }
    public DateTime PhotoDate { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DaysAfterPlanting { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CropName { get; set; }
}

public class CreateCropPhotoRequestDto
{
    public Guid CropId { get; set; }
    public DateTime PhotoDate { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateCropPhotoRequestDto
{
    public DateTime? PhotoDate { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Description { get; set; }
}

public class CropPhotoQueryRequestDto : PagedRequest
{
    public Guid? CropId { get; set; }
    public DateTime? PhotoDateFrom { get; set; }
    public DateTime? PhotoDateTo { get; set; }
}

public class CropPhotoTimelineDto
{
    public DateTime Date { get; set; }
    public List<CropPhotoDto> Photos { get; set; } = new();
}

public class CropGrowthTimelineDto
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public int TotalDays { get; set; }
    public int PhotoCount { get; set; }
    public List<CropPhotoTimelineDto> Timeline { get; set; } = new();
}
