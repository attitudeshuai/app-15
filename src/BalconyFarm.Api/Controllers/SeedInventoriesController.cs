using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedInventoriesController : ControllerBase
{
    private readonly ISeedInventoryService _seedInventoryService;

    public SeedInventoriesController(ISeedInventoryService seedInventoryService)
    {
        _seedInventoryService = seedInventoryService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<SeedInventoryDto>>>> GetSeedInventories([FromQuery] SeedInventoryQueryRequestDto query, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.GetMySeedInventoriesAsync(query, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeedInventoryDto>>> CreateSeedInventory([FromBody] CreateSeedInventoryRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.CreateSeedInventoryAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetSeedInventoryById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeedInventoryDto>>> GetSeedInventoryById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.GetSeedInventoryByIdAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeedInventoryDto>>> UpdateSeedInventory([FromRoute] Guid id, [FromBody] UpdateSeedInventoryRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.UpdateSeedInventoryAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteSeedInventory([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.DeleteSeedInventoryAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpPatch("{id}/use")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<SeedInventoryDto>>> UseSeed([FromRoute] Guid id, [FromBody] UseSeedRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.UseSeedAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpGet("expiring")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<SeedInventoryDto>>>> GetExpiringSeeds([FromQuery] int daysThreshold = 30, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _seedInventoryService.GetExpiringSeedsAsync(daysThreshold, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
