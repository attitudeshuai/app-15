using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityService _communityService;

    public CommunityController(ICommunityService communityService)
    {
        _communityService = communityService;
    }

    [HttpGet("questions")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<CommunityQuestionDto>>>> GetQuestions([FromQuery] QuestionQueryRequestDto query, CancellationToken cancellationToken)
    {
        var result = await _communityService.GetQuestionsAsync(query, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("questions/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<QuestionDetailDto>>> GetQuestionById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _communityService.GetQuestionByIdAsync(id, cancellationToken);
        if (result.Code != 200)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("questions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommunityQuestionDto>>> CreateQuestion([FromBody] CreateQuestionRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.CreateQuestionAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetQuestionById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("questions/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommunityQuestionDto>>> UpdateQuestion([FromRoute] Guid id, [FromBody] UpdateQuestionRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.UpdateQuestionAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpDelete("questions/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteQuestion([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.DeleteQuestionAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpPost("questions/{questionId}/replies")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommunityReplyDto>>> CreateReply([FromRoute] Guid questionId, [FromBody] CreateReplyRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.CreateReplyAsync(questionId, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : BadRequest(result);
        }
        return CreatedAtAction(nameof(GetQuestionById), new { id = questionId }, result);
    }

    [HttpPut("questions/{questionId}/replies/{replyId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommunityReplyDto>>> UpdateReply([FromRoute] Guid questionId, [FromRoute] Guid replyId, [FromBody] UpdateReplyRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.UpdateReplyAsync(replyId, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpDelete("questions/{questionId}/replies/{replyId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteReply([FromRoute] Guid questionId, [FromRoute] Guid replyId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.DeleteReplyAsync(replyId, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpPost("questions/{questionId}/replies/{replyId}/accept")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommunityReplyDto>>> AcceptReply([FromRoute] Guid questionId, [FromRoute] Guid replyId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _communityService.AcceptReplyAsync(questionId, replyId, userId, cancellationToken);
        if (result.Code != 200)
        {
            return result.Code == 404 ? NotFound(result) : StatusCode(result.Code, result);
        }
        return Ok(result);
    }

    [HttpGet("hot-topics")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<HotTopicDto>>>> GetHotTopics(CancellationToken cancellationToken)
    {
        var result = await _communityService.GetHotTopicsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("hot-tags")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<HotTagDto>>>> GetHotTags(CancellationToken cancellationToken)
    {
        var result = await _communityService.GetHotTagsAsync(cancellationToken);
        return Ok(result);
    }
}
