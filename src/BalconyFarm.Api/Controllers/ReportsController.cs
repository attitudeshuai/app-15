using BalconyFarm.Application.Models;
using BalconyFarm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace BalconyFarm.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("monthly/{year:int}/{month:int}")]
    public async Task<ActionResult<ApiResponse<PlantingReport>>> GetMonthlyReport(
        [FromRoute] int year,
        [FromRoute] int month,
        [FromQuery] Guid? plantingLocationId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _reportService.GetMonthlyReportAsync(userId, year, month, plantingLocationId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("yearly/{year:int}")]
    public async Task<ActionResult<ApiResponse<PlantingReport>>> GetYearlyReport(
        [FromRoute] int year,
        [FromQuery] Guid? plantingLocationId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _reportService.GetYearlyReportAsync(userId, year, plantingLocationId, cancellationToken);
        if (result.Code != 200)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("monthly/{year:int}/{month:int}/export")]
    public async Task<IActionResult> ExportMonthlyReport(
        [FromRoute] int year,
        [FromRoute] int month,
        [FromQuery] string format = "json",
        [FromQuery] Guid? plantingLocationId = null,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _reportService.GetMonthlyReportAsync(userId, year, month, plantingLocationId, cancellationToken);
        if (result.Code != 200 || result.Data == null)
        {
            return BadRequest(result);
        }

        return ExportReport(result.Data, format, $"{year}年{month}月种植报告");
    }

    [HttpGet("yearly/{year:int}/export")]
    public async Task<IActionResult> ExportYearlyReport(
        [FromRoute] int year,
        [FromQuery] string format = "json",
        [FromQuery] Guid? plantingLocationId = null,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.Error("用户未认证", 401));
        }

        var result = await _reportService.GetYearlyReportAsync(userId, year, plantingLocationId, cancellationToken);
        if (result.Code != 200 || result.Data == null)
        {
            return BadRequest(result);
        }

        return ExportReport(result.Data, format, $"{year}年度种植报告");
    }

    private IActionResult ExportReport(PlantingReport report, string format, string fileNameBase)
    {
        format = format.ToLower();

        if (format == "csv")
        {
            var csv = GenerateCsvReport(report);
            var bytes = Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv; charset=utf-8", $"{fileNameBase}.csv");
        }

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        return File(jsonBytes, "application/json; charset=utf-8", $"{fileNameBase}.json");
    }

    private static string GenerateCsvReport(PlantingReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"报告标题,{report.ReportTitle}");
        sb.AppendLine($"报告周期,{report.PeriodStart:yyyy-MM-dd} 至 {report.PeriodEnd:yyyy-MM-dd}");
        sb.AppendLine($"生成时间,{report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("=== 摘要 ===");
        foreach (var highlight in report.SummaryHighlights)
        {
            sb.AppendLine($"亮点,{highlight}");
        }
        sb.AppendLine($"作物总数,{report.TotalCrops}");
        sb.AppendLine($"本期新增作物,{report.NewCropsInPeriod}");
        sb.AppendLine();

        sb.AppendLine("=== 作物列表 ===");
        sb.AppendLine("作物名称,品种,种植日期,位置,容器类型,状态,生长天数");
        foreach (var crop in report.Crops)
        {
            sb.AppendLine($"{EscapeCsv(crop.Name)},{EscapeCsv(crop.Variety)},{crop.PlantingDate:yyyy-MM-dd},{EscapeCsv(crop.Location)},{EscapeCsv(crop.ContainerType)},{crop.StatusName},{crop.GrowthDays}");
        }
        sb.AppendLine();

        sb.AppendLine("=== 收成统计 ===");
        sb.AppendLine($"总收成次数,{report.HarvestStats.TotalHarvestRecords}");
        sb.AppendLine($"总收成量,{report.HarvestStats.TotalHarvestQuantity}");
        sb.AppendLine();
        sb.AppendLine("收成明细(按作物)");
        sb.AppendLine("作物名称,收成次数,总产量,单位,平均质量评分");
        foreach (var h in report.HarvestStats.ByCrop)
        {
            sb.AppendLine($"{EscapeCsv(h.CropName)},{h.HarvestCount},{h.TotalQuantity},{EscapeCsv(h.Unit)},{h.AverageQualityScore}");
        }
        sb.AppendLine();
        sb.AppendLine("收成质量分布");
        sb.AppendLine("质量等级,次数,占比(%),总产量");
        foreach (var q in report.HarvestStats.ByQuality)
        {
            sb.AppendLine($"{q.QualityName},{q.Count},{q.Percentage},{q.TotalQuantity}");
        }
        sb.AppendLine();

        sb.AppendLine("=== 任务完成率 ===");
        sb.AppendLine($"总任务数,{report.TaskStats.TotalTasks}");
        sb.AppendLine($"已完成,{report.TaskStats.CompletedTasks}");
        sb.AppendLine($"待处理/进行中,{report.TaskStats.PendingTasks}");
        sb.AppendLine($"已逾期,{report.TaskStats.OverdueTasks}");
        sb.AppendLine($"已取消,{report.TaskStats.CancelledTasks}");
        sb.AppendLine($"完成率(%),{report.TaskStats.CompletionRate}");
        sb.AppendLine($"按时完成率(%),{report.TaskStats.OnTimeRate}");
        sb.AppendLine();
        sb.AppendLine("任务明细(按类型)");
        sb.AppendLine("任务类型,总数,已完成,完成率(%)");
        foreach (var t in report.TaskStats.ByType)
        {
            sb.AppendLine($"{t.TaskTypeName},{t.Total},{t.Completed},{t.CompletionRate}");
        }
        sb.AppendLine();
        sb.AppendLine("任务明细(按作物)");
        sb.AppendLine("作物名称,总任务数,已完成,完成率(%)");
        foreach (var t in report.TaskStats.ByCrop)
        {
            sb.AppendLine($"{EscapeCsv(t.CropName)},{t.TotalTasks},{t.CompletedTasks},{t.CompletionRate}");
        }
        sb.AppendLine();

        sb.AppendLine("=== 病虫害记录 ===");
        sb.AppendLine($"总记录数,{report.PestStats.TotalPestRecords}");
        sb.AppendLine($"未解决,{report.PestStats.ActivePestRecords}");
        sb.AppendLine($"已解决,{report.PestStats.ResolvedPestRecords}");
        sb.AppendLine($"解决率(%),{report.PestStats.ResolutionRate}");
        sb.AppendLine();
        sb.AppendLine("病虫害明细");
        sb.AppendLine("作物名称,问题类型,症状,处理方案,发现日期,解决日期,状态,持续天数");
        foreach (var p in report.PestStats.Records)
        {
            var resolvedDate = p.ResolvedDate.HasValue ? p.ResolvedDate.Value.ToString("yyyy-MM-dd") : "";
            sb.AppendLine($"{EscapeCsv(p.CropName)},{EscapeCsv(p.IssueType)},{EscapeCsv(p.Symptoms)},{EscapeCsv(p.Treatment)},{p.DetectedDate:yyyy-MM-dd},{resolvedDate},{p.StatusName},{p.DurationDays}");
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
