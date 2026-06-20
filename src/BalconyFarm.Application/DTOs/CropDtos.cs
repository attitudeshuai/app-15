using BalconyFarm.Domain.Enums;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class CropDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public Guid? PlantingLocationId { get; set; }
    public string? PlantingLocationName { get; set; }
    public string ContainerType { get; set; } = string.Empty;
    public CropStatus Status { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OwnerUsername { get; set; }
}

public class CreateCropRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public DateTime PlantingDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public Guid? PlantingLocationId { get; set; }
    public string ContainerType { get; set; } = string.Empty;
    public CropStatus Status { get; set; } = CropStatus.Growing;
    public string? PhotoUrl { get; set; }
    public string? PlantingPlanTemplateId { get; set; }
    public bool AutoGenerateTasksFromTemplate { get; set; } = true;
}

public class CreateCropWithTemplateResultDto
{
    public CropDto Crop { get; set; } = new();
    public string? AppliedTemplateId { get; set; }
    public string? AppliedTemplateName { get; set; }
    public int GeneratedTaskCount { get; set; }
    public List<CropCareTaskDto> GeneratedTasks { get; set; } = new();
}

public class UpdateCropRequestDto
{
    public string? Name { get; set; }
    public string? Variety { get; set; }
    public DateTime? PlantingDate { get; set; }
    public string? Location { get; set; }
    public Guid? PlantingLocationId { get; set; }
    public string? ContainerType { get; set; }
    public CropStatus? Status { get; set; }
    public string? PhotoUrl { get; set; }
}

public class UpdateCropStatusRequestDto
{
    public CropStatus Status { get; set; }
}

public class CropQueryRequestDto : PagedRequest
{
    public CropStatus? Status { get; set; }
    public string? Location { get; set; }
    public Guid? PlantingLocationId { get; set; }
    public string? ContainerType { get; set; }
    public DateTime? PlantingDateFrom { get; set; }
    public DateTime? PlantingDateTo { get; set; }
}

public class CropShareCardDto
{
    public Guid CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public int GrowthDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? OwnerUsername { get; set; }
    public HarvestSummaryDto HarvestSummary { get; set; } = new();
}

public class HarvestSummaryDto
{
    public int TotalHarvestCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? LatestQualityNote { get; set; }
}
