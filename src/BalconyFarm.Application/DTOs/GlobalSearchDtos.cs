using BalconyFarm.Domain.Enums;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public enum SearchResultType
{
    Crop,
    CropCareTask,
    PestRecord,
    HarvestRecord,
    SeedInventory
}

public class GlobalSearchResultItemDto
{
    public Guid Id { get; set; }
    public SearchResultType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid? CropId { get; set; }
    public string? CropName { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class GlobalSearchRequestDto : PagedRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public CropStatus? CropStatus { get; set; }
    public TaskType? TaskType { get; set; }
    public Domain.Enums.TaskStatus? TaskStatus { get; set; }
    public PestStatus? PestStatus { get; set; }
    public List<SearchResultType>? SearchTypes { get; set; }
}

public class GlobalSearchResultDto
{
    public PagedResult<GlobalSearchResultItemDto> Results { get; set; } = new();
    public Dictionary<SearchResultType, int> CountByType { get; set; } = new();
}
