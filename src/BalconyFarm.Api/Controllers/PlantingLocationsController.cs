using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantingLocationsController : ControllerBase
{
    private readonly IPlantingLocationService _plantingLocationService;

    public PlantingLocationsController(IPlantingLocationService plantingLocationService)
    {
        _plantingLocationService = plantingLocationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlantingLocationDto>>>> GetPlantingLocations(CancellationToken cancellationToken)
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _plantingLocationService.GetPlantingLocationsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlantingLocationDto>>>> GetMyPlantingLocations(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _plantingLocationService.GetMyPlantingLocationsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlantingLocationDto>>> GetPlantingLocationById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _plantingLocationService.GetPlantingLocationByIdAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PlantingLocationDto>>> CreatePlantingLocation(
        [FromBody] CreatePlantingLocationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _plantingLocationService.CreatePlantingLocationAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetPlantingLocationById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PlantingLocationDto>>> UpdatePlantingLocation(
        [FromRoute] Guid id,
        [FromBody] UpdatePlantingLocationRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _plantingLocationService.UpdatePlantingLocationAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeletePlantingLocation([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _plantingLocationService.DeletePlantingLocationAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("stats")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlantingLocationStatsDto>>>> GetLocationStats(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _plantingLocationService.GetLocationStatsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
