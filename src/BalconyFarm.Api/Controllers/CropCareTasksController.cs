using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CropCareTasksController : ControllerBase
{
    private readonly ICropCareTaskService _cropCareTaskService;
    private readonly ISmartCareRecommendationService _smartCareRecommendationService;
    private readonly IWeatherAwareTaskAdjustmentService _weatherAdjustmentService;
    private readonly IWeatherForecastService _weatherForecastService;

    public CropCareTasksController(
        ICropCareTaskService cropCareTaskService,
        ISmartCareRecommendationService smartCareRecommendationService,
        IWeatherAwareTaskAdjustmentService weatherAdjustmentService,
        IWeatherForecastService weatherForecastService)
    {
        _cropCareTaskService = cropCareTaskService;
        _smartCareRecommendationService = smartCareRecommendationService;
        _weatherAdjustmentService = weatherAdjustmentService;
        _weatherForecastService = weatherForecastService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<CropCareTaskDto>>>> GetCropCareTasks([FromQuery] CropCareTaskQueryRequestDto query, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.GetCropCareTasksAsync(query, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropCareTaskDto>>> CreateCropCareTask([FromBody] CreateCropCareTaskRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.CreateCropCareTaskAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetCropCareTaskById), new { id = result.Data?.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropCareTaskDto>>> GetCropCareTaskById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.GetCropCareTaskByIdAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropCareTaskDto>>> UpdateCropCareTask([FromRoute] Guid id, [FromBody] UpdateCropCareTaskRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.UpdateCropCareTaskAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteCropCareTask([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.DeleteCropCareTaskAsync(id, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CropCareTaskDto>>> UpdateTaskStatus([FromRoute] Guid id, [FromBody] UpdateTaskStatusRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.UpdateTaskStatusAsync(id, dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPatch("batch/status")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BatchUpdateTaskStatusResultDto>>> BatchUpdateTaskStatus([FromBody] BatchUpdateTaskStatusRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _cropCareTaskService.BatchUpdateTaskStatusAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("preview")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GenerateCareTasksResultDto>>> PreviewCareTasks([FromBody] GenerateCareTasksRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _smartCareRecommendationService.PreviewCareTasksAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GenerateCareTasksResultDto>>> GenerateCareTasks([FromBody] GenerateCareTasksRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _smartCareRecommendationService.GenerateCareTasksAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("weather-adjust")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<WeatherAdjustTaskResultDto>>> WeatherAdjustTasks([FromBody] WeatherAdjustTaskRequestDto dto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _weatherAdjustmentService.AdjustUpcomingWateringTasksAsync(dto, userId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("weather-forecast/{cityName}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<WeatherForecastDto>>> GetWeatherForecast(
        [FromRoute] string cityName,
        [FromQuery] int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out _))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var forecast = await _weatherForecastService.GetForecastAsync(cityName, daysAhead, cancellationToken);
        if (forecast == null)
        {
            return BadRequest(ApiResponse<WeatherForecastDto>.Error($"无法获取城市 {cityName} 的天气预报数据", 400));
        }
        return Ok(ApiResponse<WeatherForecastDto>.Success(forecast));
    }

    [HttpGet("weather-assess")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<WeatherImpactAssessment>>> AssessWateringByWeather(
        [FromQuery] string cityName,
        [FromQuery] DateTime scheduledDate,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out _))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var assessment = await _weatherForecastService.AssessWateringNeedAsync(cityName, scheduledDate, cancellationToken);
        return Ok(ApiResponse<WeatherImpactAssessment>.Success(assessment));
    }
}
