using BalconyFarm.Application.DTOs;

namespace BalconyFarm.Application.Services;

public interface IWeatherForecastService
{
    Task<WeatherForecastDto?> GetForecastAsync(string cityName, int daysAhead, CancellationToken cancellationToken = default);
    Task<DailyWeatherDto?> GetDailyWeatherAsync(string cityName, DateTime date, CancellationToken cancellationToken = default);
    Task<WeatherImpactAssessment> AssessWateringNeedAsync(string cityName, DateTime scheduledDate, CancellationToken cancellationToken = default);
}

public class WeatherImpactAssessment
{
    public bool ShouldSkipWatering { get; set; }
    public bool ShouldDelayWatering { get; set; }
    public int? DelayDays { get; set; }
    public string AdjustmentReason { get; set; } = string.Empty;
    public DailyWeatherDto? WeatherOnScheduledDate { get; set; }
    public List<DailyWeatherDto> ForecastDays { get; set; } = new();
}
