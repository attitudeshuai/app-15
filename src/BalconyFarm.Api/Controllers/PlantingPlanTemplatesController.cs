using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantingPlanTemplatesController : ControllerBase
{
    private readonly IPlantingPlanTemplateService _templateService;

    public PlantingPlanTemplatesController(IPlantingPlanTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<PlantingPlanTemplateDto>>>> GetAllTemplates(
        [FromQuery] PlantingPlanTemplateQueryRequestDto query,
        CancellationToken cancellationToken)
    {
        var result = await _templateService.GetAllTemplatesAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<PlantingPlanTemplateDto>>>> SearchTemplates(
        [FromQuery] PlantingPlanTemplateQueryRequestDto query,
        CancellationToken cancellationToken)
    {
        var result = await _templateService.SearchTemplatesAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlantingPlanTemplateDto>>> GetTemplateById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await _templateService.GetTemplateByIdAsync(id, cancellationToken);
        if (result.Code != 200)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("by-crop/{cropName}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PlantingPlanTemplateDto>>> GetTemplateByCropName(
        [FromRoute] string cropName,
        CancellationToken cancellationToken)
    {
        var result = await _templateService.GetTemplateByCropNameAsync(cropName, cancellationToken);
        if (result.Code != 200)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ApplyTemplateResultDto>>> ApplyTemplate(
        [FromBody] ApplyTemplateRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _templateService.ApplyTemplateAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
