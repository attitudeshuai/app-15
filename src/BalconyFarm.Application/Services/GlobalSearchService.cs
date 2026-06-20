using System.Linq.Expressions;
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

        var userCropIdNameMap = await GetUserCropIdNameMapAsync(userId, cancellationToken);
        var userCropIds = userCropIdNameMap.Keys.ToHashSet();

        HashSet<Guid>? keywordMatchedCropIds = null;
        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            var keyword = query.SearchKeyword;
            keywordMatchedCropIds = new HashSet<Guid>(
                userCropIdNameMap.Where(kv => kv.Value.Contains(keyword))
                    .Select(kv => kv.Key)
            );
        }

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
            var tasks = await SearchCropCareTasksAsync(query, userCropIds, keywordMatchedCropIds, userCropIdNameMap, cancellationToken);
            countByType[SearchResultType.CropCareTask] = tasks.Count;
            allResults.AddRange(tasks);
        }

        if (searchTypes.Contains(SearchResultType.PestRecord))
        {
            var pests = await SearchPestRecordsAsync(query, userCropIds, keywordMatchedCropIds, userCropIdNameMap, cancellationToken);
            countByType[SearchResultType.PestRecord] = pests.Count;
            allResults.AddRange(pests);
        }

        if (searchTypes.Contains(SearchResultType.HarvestRecord))
        {
            var harvests = await SearchHarvestRecordsAsync(query, userCropIds, keywordMatchedCropIds, userCropIdNameMap, cancellationToken);
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

    private async Task<Dictionary<Guid, string>> GetUserCropIdNameMapAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userCrops = await _unitOfWork.Crops.FindAsync(c => c.UserId == userId, cancellationToken);
        return userCrops.ToDictionary(c => c.Id, c => c.Name);
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchCropsAsync(GlobalSearchRequestDto query, Guid userId, CancellationToken cancellationToken)
    {
        Expression<Func<Crop, bool>> predicate = c => c.UserId == userId;

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            var keyword = query.SearchKeyword;
            predicate = predicate.And(c =>
                c.Name.Contains(keyword) ||
                c.Variety.Contains(keyword) ||
                c.Location.Contains(keyword));
        }

        if (query.CropStatus.HasValue)
        {
            var status = query.CropStatus.Value;
            predicate = predicate.And(c => c.Status == status);
        }

        if (query.DateFrom.HasValue)
        {
            var dateFrom = query.DateFrom.Value;
            predicate = predicate.And(c => c.PlantingDate >= dateFrom || c.CreatedAt >= dateFrom);
        }

        if (query.DateTo.HasValue)
        {
            var dateTo = query.DateTo.Value;
            predicate = predicate.And(c => c.PlantingDate <= dateTo || c.CreatedAt <= dateTo);
        }

        var crops = await _unitOfWork.Crops.FindAsync(predicate, cancellationToken);

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

    private async Task<List<GlobalSearchResultItemDto>> SearchCropCareTasksAsync(
        GlobalSearchRequestDto query,
        HashSet<Guid> userCropIds,
        HashSet<Guid>? keywordMatchedCropIds,
        Dictionary<Guid, string> cropIdNameMap,
        CancellationToken cancellationToken)
    {
        Expression<Func<CropCareTask, bool>> predicate = t => userCropIds.Contains(t.CropId);

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            var keyword = query.SearchKeyword;
            var hasKeywordMatchedCrops = keywordMatchedCropIds != null && keywordMatchedCropIds.Count > 0;

            if (hasKeywordMatchedCrops)
            {
                predicate = predicate.And(t =>
                    (t.Note != null && t.Note.Contains(keyword)) ||
                    keywordMatchedCropIds!.Contains(t.CropId));
            }
            else
            {
                predicate = predicate.And(t => t.Note != null && t.Note.Contains(keyword));
            }
        }

        if (query.TaskType.HasValue)
        {
            var taskType = query.TaskType.Value;
            predicate = predicate.And(t => t.TaskType == taskType);
        }

        if (query.TaskStatus.HasValue)
        {
            var taskStatus = query.TaskStatus.Value;
            predicate = predicate.And(t => t.Status == taskStatus);
        }

        if (query.DateFrom.HasValue)
        {
            var dateFrom = query.DateFrom.Value;
            predicate = predicate.And(t => t.ScheduledDate >= dateFrom ||
                                           (t.CompletedDate.HasValue && t.CompletedDate.Value >= dateFrom));
        }

        if (query.DateTo.HasValue)
        {
            var dateTo = query.DateTo.Value;
            predicate = predicate.And(t => t.ScheduledDate <= dateTo ||
                                           (t.CompletedDate.HasValue && t.CompletedDate.Value <= dateTo));
        }

        var tasks = await _unitOfWork.CropCareTasks.FindAsync(predicate, cancellationToken);

        return tasks.Select(t =>
        {
            cropIdNameMap.TryGetValue(t.CropId, out var cropName);
            cropName ??= "未知作物";
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
                    { "completedDate", t.CompletedDate ?? (object?)DBNull.Value },
                    { "isOverdue", t.Status == Domain.Enums.TaskStatus.Pending && t.ScheduledDate < DateTime.Now }
                }
            };
        }).ToList();
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchPestRecordsAsync(
        GlobalSearchRequestDto query,
        HashSet<Guid> userCropIds,
        HashSet<Guid>? keywordMatchedCropIds,
        Dictionary<Guid, string> cropIdNameMap,
        CancellationToken cancellationToken)
    {
        Expression<Func<PestRecord, bool>> predicate = p => userCropIds.Contains(p.CropId);

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            var keyword = query.SearchKeyword;
            var hasKeywordMatchedCrops = keywordMatchedCropIds != null && keywordMatchedCropIds.Count > 0;

            if (hasKeywordMatchedCrops)
            {
                predicate = predicate.And(p =>
                    p.IssueType.Contains(keyword) ||
                    p.Symptoms.Contains(keyword) ||
                    p.Treatment.Contains(keyword) ||
                    keywordMatchedCropIds!.Contains(p.CropId));
            }
            else
            {
                predicate = predicate.And(p =>
                    p.IssueType.Contains(keyword) ||
                    p.Symptoms.Contains(keyword) ||
                    p.Treatment.Contains(keyword));
            }
        }

        if (query.PestStatus.HasValue)
        {
            var pestStatus = query.PestStatus.Value;
            predicate = predicate.And(p => p.Status == pestStatus);
        }

        if (query.DateFrom.HasValue)
        {
            var dateFrom = query.DateFrom.Value;
            predicate = predicate.And(p => p.DetectedDate >= dateFrom ||
                                           (p.ResolvedDate.HasValue && p.ResolvedDate.Value >= dateFrom));
        }

        if (query.DateTo.HasValue)
        {
            var dateTo = query.DateTo.Value;
            predicate = predicate.And(p => p.DetectedDate <= dateTo ||
                                           (p.ResolvedDate.HasValue && p.ResolvedDate.Value <= dateTo));
        }

        var pests = await _unitOfWork.PestRecords.FindAsync(predicate, cancellationToken);

        return pests.Select(p =>
        {
            cropIdNameMap.TryGetValue(p.CropId, out var cropName);
            cropName ??= "未知作物";
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
                    { "resolvedDate", p.ResolvedDate ?? (object?)DBNull.Value }
                }
            };
        }).ToList();
    }

    private async Task<List<GlobalSearchResultItemDto>> SearchHarvestRecordsAsync(
        GlobalSearchRequestDto query,
        HashSet<Guid> userCropIds,
        HashSet<Guid>? keywordMatchedCropIds,
        Dictionary<Guid, string> cropIdNameMap,
        CancellationToken cancellationToken)
    {
        Expression<Func<HarvestRecord, bool>> predicate = h => userCropIds.Contains(h.CropId);

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            var keyword = query.SearchKeyword;
            var hasKeywordMatchedCrops = keywordMatchedCropIds != null && keywordMatchedCropIds.Count > 0;

            if (hasKeywordMatchedCrops)
            {
                predicate = predicate.And(h =>
                    h.Unit.Contains(keyword) ||
                    (h.QualityNote != null && h.QualityNote.Contains(keyword)) ||
                    keywordMatchedCropIds!.Contains(h.CropId));
            }
            else
            {
                predicate = predicate.And(h =>
                    h.Unit.Contains(keyword) ||
                    (h.QualityNote != null && h.QualityNote.Contains(keyword)));
            }
        }

        if (query.DateFrom.HasValue)
        {
            var dateFrom = query.DateFrom.Value;
            predicate = predicate.And(h => h.HarvestDate >= dateFrom);
        }

        if (query.DateTo.HasValue)
        {
            var dateTo = query.DateTo.Value;
            predicate = predicate.And(h => h.HarvestDate <= dateTo);
        }

        var harvests = await _unitOfWork.HarvestRecords.FindAsync(predicate, cancellationToken);

        return harvests.Select(h =>
        {
            cropIdNameMap.TryGetValue(h.CropId, out var cropName);
            cropName ??= "未知作物";
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

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftExpr = leftVisitor.Visit(left.Body);
        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightExpr = rightVisitor.Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftExpr!, rightExpr!), parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
