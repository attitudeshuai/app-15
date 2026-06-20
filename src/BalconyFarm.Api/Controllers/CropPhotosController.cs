using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CropPhotosController : ControllerBase
{
    private readonly ICropPhotoService _cropPhotoService;

    public CropPhotosController(ICropPhotoService cropPhotoService)
    {
        _cropPhotoService = cropPhotoService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<CropPhotoDto>>>> GetPhotos([FromQuery] CropPhotoQueryRequestDto query, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.GetPhotosAsync(query, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropPhotoDto>>> GetPhotoById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.GetPhotoByIdAsync(id, userId, cancellationToken);
        if (result.Code == 404)
        {
            return NotFound(result);
        }
        if (result.Code == 403)
        {
            return Forbid();
        }
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropPhotoDto>>> CreatePhoto([FromBody] CreateCropPhotoRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.CreatePhotoAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetPhotoById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropPhotoDto>>> UpdatePhoto([FromRoute] Guid id, [FromBody] UpdateCropPhotoRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.UpdatePhotoAsync(id, dto, userId, cancellationToken);
        if (result.Code == 404)
        {
            return NotFound(result);
        }
        if (result.Code == 403)
        {
            return Forbid();
        }
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeletePhoto([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.DeletePhotoAsync(id, userId, cancellationToken);
        if (result.Code == 404)
        {
            return NotFound(result);
        }
        if (result.Code == 403)
        {
            return Forbid();
        }
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("timeline/{cropId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropGrowthTimelineDto>>> GetGrowthTimeline([FromRoute] Guid cropId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropPhotoService.GetGrowthTimelineAsync(cropId, userId, cancellationToken);
        if (result.Code == 404)
        {
            return NotFound(result);
        }
        if (result.Code == 403)
        {
            return Forbid();
        }
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
