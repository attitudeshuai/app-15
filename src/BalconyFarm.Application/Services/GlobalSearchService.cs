using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Enums;
using BalconyFarm.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GlobalSearchService> _logger;

    public GlobalSearchService(IUnitOfWork unitOfWork, ILogger<GlobalSearchService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<GlobalSearchResultDto>> SearchAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("全局搜索: 用户={UserId}, 关键词={Keyword}", userId, query.SearchKeyword);

        var searchTypes = query.SearchTypes ?? new List<SearchResultType>
        {
            SearchResultType.Crop,
            SearchResultType.CropCareTask,
            SearchResultType.PestRecord,
            SearchResultType.HarvestRecord
        };

        var allResults = new List<GlobalSearchResultItemDto>();
        var countByType = new Dictionary<SearchResultType, int>();

        if (searchTypes.Contains(SearchResultType.Crop))
        {
            var crops = await SearchCropsAsync(query, userId, cancellationToken);
            countByType[SearchResultType.Crop] = crops.Count;
            allResults.AddRange(crops);
        }

        if (searchTypes.Contains(SearchResultType.CropCareTask))
        {
            var tasks = await SearchCropCareTasksAsync(query, userId, cancellationToken);
            countByType[SearchResultType.CropCareTask] = tasks.Count;
            allResults.AddRange(tasks);
        }

        if (searchTypes.Contains(SearchResultType.PestRecord))
        {
            var pests = await SearchPestRecordsAsync(query, userId, cancellationToken);
            countByType[SearchResultType.PestRecord] = pests.Count;
            allResults.AddRange(pests);
        }

        if (searchTypes.Contains(SearchResultType.HarvestRecord))
        {
            var harvests = await SearchHarvestRecordsAsync(query, userId, cancellationToken);
            countByType[SearchResultType.HarvestRecord] = harvests.Count;
            allResults.AddRange(harvests);
        }

        var totalCount = allResults.Count;

        var sortedResults = query.SortOrder?.ToLower() == "asc"
            ? allResults.OrderBy(r => r.Date)
            : allResults.OrderByDescending(r => r.Date);

        var items = sortedResults
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var result = new GlobalSearchResultDto
        {
            Results = new PagedResult<GlobalSearchResultItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            },
            CountByType = countByType
        };

        return ApiResponse<GlobalSearchResultDto>.Success(result);
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchCropsAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken)
    {
        var crops = (await _unitOfWork.Crops.GetAllAsync(cancellationToken))
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            crops = crops.Where(c =>
                c.Name.Contains(query.SearchKeyword) ||
                c.Variety.Contains(query.SearchKeyword) ||
                c.Location.Contains(query.SearchKeyword));
        }

        if (query.CropStatus.HasValue)
        {
            crops = crops.Where(c => c.Status == query.CropStatus.Value);
        }

        if (query.DateFrom.HasValue)
        {
            crops = crops.Where(c => c.PlantingDate >= query.DateFrom.Value || c.CreatedAt >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            crops = crops.Where(c => c.PlantingDate <= query.DateTo.Value || c.CreatedAt <= query.DateTo.Value);
        }

        return crops.Select(c => new GlobalSearchResultItemDto
        {
            Id = c.Id,
            Type = SearchResultType.Crop,
            Title = c.Name,
            Description = $"品种: {c.Variety} | 位置: {c.Location}",
            Date = c.PlantingDate,
            CropId = c.Id,
            CropName = c.Name,
            Status = c.Status.ToString(),
            Metadata = new Dictionary<string, object>
            {
                { "variety", c.Variety },
                { "location", c.Location },
                { "containerType", c.ContainerType },
                { "plantingDate", c.PlantingDate },
                { "photoUrl", c.PhotoUrl ?? string.Empty }
            }
        }).ToList();
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchCropCareTasksAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken)
    {
        var allCrops = (await _unitOfWork.Crops.GetAllAsync(cancellationToken))
            .Where(c => c.UserId == userId)
            .ToDictionary(c => c.Id, c => c);

        var cropIds = allCrops.Keys.ToHashSet();

        var tasks = (await _unitOfWork.CropCareTasks.GetAllAsync(cancellationToken))
            .Where(t => cropIds.Contains(t.CropId));

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            tasks = tasks.Where(t =>
                t.Note != null && t.Note.Contains(query.SearchKeyword));
        }

        if (query.TaskType.HasValue)
        {
            tasks = tasks.Where(t => t.TaskType == query.TaskType.Value);
        }

        if (query.TaskStatus.HasValue)
        {
            tasks = tasks.Where(t => t.Status == query.TaskStatus.Value);
        }

        if (query.DateFrom.HasValue)
        {
            tasks = tasks.Where(t => t.ScheduledDate >= query.DateFrom.Value ||
                                     (t.CompletedDate.HasValue && t.CompletedDate.Value >= query.DateFrom.Value));
        }

        if (query.DateTo.HasValue)
        {
            tasks = tasks.Where(t => t.ScheduledDate <= query.DateTo.Value ||
                                     (t.CompletedDate.HasValue && t.CompletedDate.Value <= query.DateTo.Value));
        }

        return tasks.Select(t =>
        {
            allCrops.TryGetValue(t.CropId, out var crop);
            var cropName = crop?.Name ?? "未知作物";
            return new GlobalSearchResultItemDto
            {
                Id = t.Id,
                Type = SearchResultType.CropCareTask,
                Title = $"{GetTaskTypeName(t.TaskType)} - {cropName}",
                Description = t.Note ?? string.Empty,
                Date = t.CompletedDate ?? t.ScheduledDate,
                CropId = t.CropId,
                CropName = cropName,
                Status = t.Status.ToString(),
                Metadata = new Dictionary<string, object>
                {
                    { "taskType", t.TaskType },
                    { "scheduledDate", t.ScheduledDate },
                    { "completedDate", t.CompletedDate ?? (object?)null },
                    { "isOverdue", t.Status == Domain.Enums.TaskStatus.Pending && t.ScheduledDate < DateTime.Now }
                }
            };
        }).ToList();
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchPestRecordsAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken)
    {
        var allCrops = (await _unitOfWork.Crops.GetAllAsync(cancellationToken))
            .Where(c => c.UserId == userId)
            .ToDictionary(c => c.Id, c => c);

        var cropIds = allCrops.Keys.ToHashSet();

        var pests = (await _unitOfWork.PestRecords.GetAllAsync(cancellationToken))
            .Where(p => cropIds.Contains(p.CropId));

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            pests = pests.Where(p =>
                p.IssueType.Contains(query.SearchKeyword) ||
                p.Symptoms.Contains(query.SearchKeyword) ||
                p.Treatment.Contains(query.SearchKeyword));
        }

        if (query.PestStatus.HasValue)
        {
            pests = pests.Where(p => p.Status == query.PestStatus.Value);
        }

        if (query.DateFrom.HasValue)
        {
            pests = pests.Where(p => p.DetectedDate >= query.DateFrom.Value ||
                                     (p.ResolvedDate.HasValue && p.ResolvedDate.Value >= query.DateFrom.Value));
        }

        if (query.DateTo.HasValue)
        {
            pests = pests.Where(p => p.DetectedDate <= query.DateTo.Value ||
                                     (p.ResolvedDate.HasValue && p.ResolvedDate.Value <= query.DateTo.Value));
        }

        return pests.Select(p =>
        {
            allCrops.TryGetValue(p.CropId, out var crop);
            var cropName = crop?.Name ?? "未知作物";
            return new GlobalSearchResultItemDto
            {
                Id = p.Id,
                Type = SearchResultType.PestRecord,
                Title = $"{p.IssueType} - {cropName}",
                Description = $"症状: {p.Symptoms} | 处理: {p.Treatment}",
                Date = p.ResolvedDate ?? p.DetectedDate,
                CropId = p.CropId,
                CropName = cropName,
                Status = p.Status.ToString(),
                Metadata = new Dictionary<string, object>
                {
                    { "issueType", p.IssueType },
                    { "symptoms", p.Symptoms },
                    { "treatment", p.Treatment },
                    { "detectedDate", p.DetectedDate },
                    { "resolvedDate", p.ResolvedDate ?? (object?)null }
                }
            };
        }).ToList();
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchHarvestRecordsAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken)
    {
        var allCrops = (await _unitOfWork.Crops.GetAllAsync(cancellationToken))
            .Where(c => c.UserId == userId)
            .ToDictionary(c => c.Id, c => c);

        var cropIds = allCrops.Keys.ToHashSet();

        var harvests = (await _unitOfWork.HarvestRecords.GetAllAsync(cancellationToken))
            .Where(h => cropIds.Contains(h.CropId));

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            harvests = harvests.Where(h =>
                h.Unit.Contains(query.SearchKeyword) ||
                (h.QualityNote != null && h.QualityNote.Contains(query.SearchKeyword)));
        }

        if (query.DateFrom.HasValue)
        {
            harvests = harvests.Where(h => h.HarvestDate >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            harvests = harvests.Where(h => h.HarvestDate <= query.DateTo.Value);
        }

        return harvests.Select(h =>
        {
            allCrops.TryGetValue(h.CropId, out var crop);
            var cropName = crop?.Name ?? "未知作物";
            return new GlobalSearchResultItemDto
            {
                Id = h.Id,
                Type = SearchResultType.HarvestRecord,
                Title = $"收获 - {cropName}",
                Description = $"产量: {h.Quantity} {h.Unit} | 品质: {h.QualityNote ?? "未记录"}",
                Date = h.HarvestDate,
                CropId = h.CropId,
                CropName = cropName,
                Status = "已完成",
                Metadata = new Dictionary<string, object>
                {
                    { "quantity", h.Quantity },
                    { "unit", h.Unit },
                    { "qualityNote", h.QualityNote ?? string.Empty },
                    { "harvestDate", h.HarvestDate },
                    { "photoUrl", h.PhotoUrl ?? string.Empty }
                }
            };
        }).ToList();
    }

    private static string GetTaskTypeName(TaskType taskType)
    {
        return taskType switch
        {
            TaskType.Water => "浇水",
            TaskType.Fertilize => "施肥",
            TaskType.Prune => "修剪",
            TaskType.Repot => "换盆",
            _ => taskType.ToString()
        };
    }
}
