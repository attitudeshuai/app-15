using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class SeedInventoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysToExpiry { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool IsExpired { get; set; }
}

public class CreateSeedInventoryRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Variety { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSeedInventoryRequestDto
{
    public string? Name { get; set; }
    public string? Variety { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class SeedInventoryQueryRequestDto : PagedRequest
{
    public bool? IsExpiringSoon { get; set; }
    public bool? IsExpired { get; set; }
    public int? DaysToExpiryThreshold { get; set; }
    public DateTime? ExpiryDateFrom { get; set; }
    public DateTime? ExpiryDateTo { get; set; }
}

public class UseSeedRequestDto
{
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
