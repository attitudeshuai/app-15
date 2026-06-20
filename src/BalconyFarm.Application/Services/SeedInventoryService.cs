using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class SeedInventoryService : ISeedInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedInventoryService> _logger;
    private const int DefaultExpiryThresholdDays = 30;

    public SeedInventoryService(IUnitOfWork unitOfWork, ILogger<SeedInventoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetSeedInventoriesAsync(SeedInventoryQueryRequestDto query, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var seedsList = (await _unitOfWork.SeedInventories.GetAllAsync(cancellationToken)).ToList();

        IEnumerable<SeedInventory> seeds = seedsList;

        if (userId.HasValue)
        {
            seeds = seeds.Where(s => s.UserId == userId.Value);
        }

        var today = DateTime.UtcNow.Date;
        var thresholdDays = query.DaysToExpiryThreshold ?? DefaultExpiryThresholdDays;

        if (query.IsExpiringSoon.HasValue)
        {
            seeds = query.IsExpiringSoon.Value
                ? seeds.Where(s => s.ExpiryDate.Date >= today && (s.ExpiryDate.Date - today).TotalDays <= thresholdDays)
                : seeds.Where(s => (s.ExpiryDate.Date - today).TotalDays > thresholdDays);
        }

        if (query.IsExpired.HasValue)
        {
            seeds = query.IsExpired.Value
                ? seeds.Where(s => s.ExpiryDate.Date < today)
                : seeds.Where(s => s.ExpiryDate.Date >= today);
        }

        if (query.ExpiryDateFrom.HasValue)
        {
            seeds = seeds.Where(s => s.ExpiryDate >= query.ExpiryDateFrom.Value);
        }

        if (query.ExpiryDateTo.HasValue)
        {
            seeds = seeds.Where(s => s.ExpiryDate <= query.ExpiryDateTo.Value);
        }

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            seeds = seeds.Where(s =>
                s.Name.Contains(query.SearchKeyword) ||
                s.Variety.Contains(query.SearchKeyword));
        }

        var totalCount = seeds.Count();
        var sortFunc = GetSortProperty(query.SortBy ?? "expirydate").Compile();

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            seeds = query.SortOrder?.ToLower() == "desc"
                ? seeds.OrderByDescending(sortFunc)
                : seeds.OrderBy(sortFunc);
        }
        else
        {
            seeds = seeds.OrderBy(s => s.ExpiryDate);
        }

        var items = seeds
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(EnrichWithExpiryInfo)
            .ToList();

        var result = new PagedResult<SeedInventoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PagedResult<SeedInventoryDto>>.Success(result);
    }

    public async Task<ApiResponse<SeedInventoryDto>> GetSeedInventoryByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var seed = await _unitOfWork.SeedInventories.GetByIdAsync(id, cancellationToken);
        if (seed == null)
        {
            return ApiResponse<SeedInventoryDto>.Error("种子库存不存在", 404);
        }

        if (userId.HasValue && seed.UserId != userId.Value)
        {
            return ApiResponse<SeedInventoryDto>.Error("无权访问此种子库存", 403);
        }

        var seedDto = EnrichWithExpiryInfo(seed);
        return ApiResponse<SeedInventoryDto>.Success(seedDto);
    }

    public async Task<ApiResponse<SeedInventoryDto>> CreateSeedInventoryAsync(CreateSeedInventoryRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建种子库存: {Name}, 用户: {UserId}", dto.Name, userId);

        var seed = dto.Adapt<SeedInventory>();
        seed.Id = Guid.NewGuid();
        seed.UserId = userId;
        seed.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.SeedInventories.AddAsync(seed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种子库存创建成功: {SeedId}", seed.Id);

        var seedDto = EnrichWithExpiryInfo(seed);
        return ApiResponse<SeedInventoryDto>.Success(seedDto, "创建成功");
    }

    public async Task<ApiResponse<SeedInventoryDto>> UpdateSeedInventoryAsync(Guid id, UpdateSeedInventoryRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新种子库存: {SeedId}, 用户: {UserId}", id, userId);

        var seed = await _unitOfWork.SeedInventories.GetByIdAsync(id, cancellationToken);
        if (seed == null)
        {
            return ApiResponse<SeedInventoryDto>.Error("种子库存不存在", 404);
        }

        if (seed.UserId != userId)
        {
            return ApiResponse<SeedInventoryDto>.Error("无权修改此种子库存", 403);
        }

        if (!string.IsNullOrEmpty(dto.Name))
            seed.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Variety))
            seed.Variety = dto.Variety;
        if (dto.Quantity.HasValue)
            seed.Quantity = dto.Quantity.Value;
        if (!string.IsNullOrEmpty(dto.Unit))
            seed.Unit = dto.Unit;
        if (dto.PurchaseDate.HasValue)
            seed.PurchaseDate = dto.PurchaseDate.Value;
        if (dto.ExpiryDate.HasValue)
            seed.ExpiryDate = dto.ExpiryDate.Value;
        if (dto.Notes != null)
            seed.Notes = dto.Notes;

        await _unitOfWork.SeedInventories.UpdateAsync(seed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种子库存更新成功: {SeedId}", id);

        var seedDto = EnrichWithExpiryInfo(seed);
        return ApiResponse<SeedInventoryDto>.Success(seedDto, "更新成功");
    }

    public async Task<ApiResponse> DeleteSeedInventoryAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除种子库存: {SeedId}, 用户: {UserId}", id, userId);

        var seed = await _unitOfWork.SeedInventories.GetByIdAsync(id, cancellationToken);
        if (seed == null)
        {
            return ApiResponse.Error("种子库存不存在", 404);
        }

        if (seed.UserId != userId)
        {
            return ApiResponse.Error("无权删除此种子库存", 403);
        }

        await _unitOfWork.SeedInventories.DeleteAsync(seed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种子库存删除成功: {SeedId}", id);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<SeedInventoryDto>> UseSeedAsync(Guid id, UseSeedRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("使用种子: {SeedId}, 数量: {Quantity}, 用户: {UserId}", id, dto.Quantity, userId);

        var seed = await _unitOfWork.SeedInventories.GetByIdAsync(id, cancellationToken);
        if (seed == null)
        {
            return ApiResponse<SeedInventoryDto>.Error("种子库存不存在", 404);
        }

        if (seed.UserId != userId)
        {
            return ApiResponse<SeedInventoryDto>.Error("无权操作此种子库存", 403);
        }

        if (dto.Quantity <= 0)
        {
            return ApiResponse<SeedInventoryDto>.Error("使用数量必须大于0", 400);
        }

        if (dto.Quantity > seed.Quantity)
        {
            return ApiResponse<SeedInventoryDto>.Error($"库存不足，当前库存: {seed.Quantity} {seed.Unit}", 400);
        }

        seed.Quantity -= dto.Quantity;

        if (!string.IsNullOrEmpty(dto.Note))
        {
            seed.Notes = string.IsNullOrEmpty(seed.Notes)
                ? $"使用 {dto.Quantity} {seed.Unit}: {dto.Note}"
                : $"{seed.Notes}; 使用 {dto.Quantity} {seed.Unit}: {dto.Note}";
        }

        await _unitOfWork.SeedInventories.UpdateAsync(seed, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("种子使用成功: {SeedId}, 剩余数量: {Quantity}", id, seed.Quantity);

        var seedDto = EnrichWithExpiryInfo(seed);
        return ApiResponse<SeedInventoryDto>.Success(seedDto, $"已使用 {dto.Quantity} {seed.Unit}");
    }

    public async Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetMySeedInventoriesAsync(SeedInventoryQueryRequestDto query, Guid userId, CancellationToken cancellationToken = default)
    {
        return await GetSeedInventoriesAsync(query, userId, cancellationToken);
    }

    public async Task<ApiResponse<PagedResult<SeedInventoryDto>>> GetExpiringSeedsAsync(int daysThreshold, Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new SeedInventoryQueryRequestDto
        {
            PageNumber = 1,
            PageSize = 100,
            IsExpiringSoon = true,
            DaysToExpiryThreshold = daysThreshold,
            SortBy = "expirydate",
            SortOrder = "asc"
        };

        return await GetSeedInventoriesAsync(query, userId, cancellationToken);
    }

    private static SeedInventoryDto EnrichWithExpiryInfo(SeedInventory seed)
    {
        var dto = seed.Adapt<SeedInventoryDto>();
        var today = DateTime.UtcNow.Date;
        var daysToExpiry = (int)(seed.ExpiryDate.Date - today).TotalDays;

        dto.DaysToExpiry = daysToExpiry;
        dto.IsExpired = daysToExpiry < 0;
        dto.IsExpiringSoon = daysToExpiry >= 0 && daysToExpiry <= DefaultExpiryThresholdDays;

        return dto;
    }

    private static System.Linq.Expressions.Expression<Func<SeedInventory, object>> GetSortProperty(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "name" => seed => seed.Name,
            "quantity" => seed => seed.Quantity,
            "purchasedate" => seed => seed.PurchaseDate,
            "expirydate" => seed => seed.ExpiryDate,
            "createdat" => seed => seed.CreatedAt,
            _ => seed => seed.ExpiryDate
        };
    }
}
