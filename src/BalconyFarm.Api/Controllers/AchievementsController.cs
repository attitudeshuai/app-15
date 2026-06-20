using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AchievementSummaryDto>>> GetMyAchievements(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<AchievementDto>>>> GetAllAchievements(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = null;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _achievementService.GetAllAchievementsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("unlocked")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<AchievementDto>>>> GetUnlockedAchievements(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _achievementService.GetUnlockedAchievementsAsync(userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
