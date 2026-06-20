using BalconyFarm.Application.Data;
using BalconyFarm.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IPlantingCalendarDataProvider _dataProvider;
    private readonly ILogger<WeatherForecastService> _logger;
    private readonly Random _random = new();

    public WeatherForecastService(
        IPlantingCalendarDataProvider dataProvider,
        ILogger<WeatherForecastService> logger)
    {
        _dataProvider = dataProvider;
        _logger = logger;
    }

    public async Task<WeatherForecastDto?> GetForecastAsync(string cityName, int daysAhead, CancellationToken cancellationToken = default)
    {
        if (daysAhead <= 0 || daysAhead > 30)
        {
            _logger.LogWarning("预报天数 {DaysAhead} 超出有效范围 (1-30)", daysAhead);
            return null;
        }

        var cities = await _dataProvider.GetCitiesAsync(cancellationToken);
        var city = FindCity(cities, cityName);
        if (city == null)
        {
            _logger.LogWarning("未找到城市 {CityName} 的气候数据", cityName);
            return null;
        }

        var forecasts = new List<DailyWeatherDto>();
        var today = DateTime.UtcNow.Date;

        for (var i = 0; i < daysAhead; i++)
        {
            var date = today.AddDays(i);
            forecasts.Add(GenerateDailyWeather(city, date));
        }

        return new WeatherForecastDto
        {
            CityName = city.Name,
            DailyForecasts = forecasts
        };
    }

    public async Task<DailyWeatherDto?> GetDailyWeatherAsync(string cityName, DateTime date, CancellationToken cancellationToken = default)
    {
        var cities = await _dataProvider.GetCitiesAsync(cancellationToken);
        var city = FindCity(cities, cityName);
        if (city == null)
        {
            _logger.LogWarning("未找到城市 {CityName} 的气候数据", cityName);
            return null;
        }

        return GenerateDailyWeather(city, date.Date);
    }

    public async Task<WeatherImpactAssessment> AssessWateringNeedAsync(string cityName, DateTime scheduledDate, CancellationToken cancellationToken = default)
    {
        var assessment = new WeatherImpactAssessment();
        var targetDate = scheduledDate.Date;

        var forecast = await GetForecastAsync(cityName, 10, cancellationToken);
        if (forecast == null)
        {
            assessment.AdjustmentReason = "无法获取该城市的天气数据，按原计划执行";
            return assessment;
        }

        assessment.ForecastDays = forecast.DailyForecasts;
        var weatherOnSchedule = forecast.DailyForecasts.FirstOrDefault(d => d.Date == targetDate);
        assessment.WeatherOnScheduledDate = weatherOnSchedule;

        if (weatherOnSchedule == null)
        {
            assessment.AdjustmentReason = "无法获取指定日期的天气数据，按原计划执行";
            return assessment;
        }

        var upcomingRainDays = forecast.DailyForecasts
            .Where(d => d.Date >= targetDate && d.Date <= targetDate.AddDays(2) && d.PrecipitationMm >= 5)
            .ToList();

        var totalRainNext3Days = forecast.DailyForecasts
            .Where(d => d.Date >= targetDate && d.Date <= targetDate.AddDays(2))
            .Sum(d => d.PrecipitationMm);

        if (weatherOnSchedule.IsHeavyRain)
        {
            assessment.ShouldSkipWatering = true;
            assessment.AdjustmentReason = $"当日有大雨（预计降雨量 {weatherOnSchedule.PrecipitationMm:F1}mm），土壤水分充足，无需浇水";
            return assessment;
        }

        if (weatherOnSchedule.PrecipitationMm >= 10)
        {
            assessment.ShouldSkipWatering = true;
            assessment.AdjustmentReason = $"当日有降雨（预计降雨量 {weatherOnSchedule.PrecipitationMm:F1}mm），可以满足作物需水，跳过浇水";
            return assessment;
        }

        if (totalRainNext3Days >= 25 && upcomingRainDays.Count >= 2)
        {
            assessment.ShouldDelayWatering = true;
            assessment.DelayDays = 1;
            assessment.AdjustmentReason = $"未来3天预计有{upcomingRainDays.Count}天降雨（累计降雨量 {totalRainNext3Days:F1}mm），建议延后1天浇水，观察降雨情况";
            return assessment;
        }

        if (weatherOnSchedule.TemperatureC >= 35)
        {
            assessment.ShouldDelayWatering = true;
            assessment.DelayDays = 0;
            assessment.AdjustmentReason = $"当日高温（预计 {weatherOnSchedule.TemperatureC:F1}°C），建议避开正午时段，选择清晨或傍晚浇水";
            return assessment;
        }

        if (weatherOnSchedule.TemperatureC <= 0)
        {
            assessment.ShouldSkipWatering = true;
            assessment.AdjustmentReason = $"当日低温（预计 {weatherOnSchedule.TemperatureC:F1}°C），土壤可能冻结，跳过浇水避免冻伤根系";
            return assessment;
        }

        if (weatherOnSchedule.PrecipitationMm >= 3 && weatherOnSchedule.PrecipitationMm < 10)
        {
            assessment.AdjustmentReason = $"当日有小雨（预计降雨量 {weatherOnSchedule.PrecipitationMm:F1}mm），可适当减少浇水量";
            return assessment;
        }

        assessment.AdjustmentReason = $"天气晴好（预计温度 {weatherOnSchedule.TemperatureC:F1}°C，无明显降雨），按计划浇水";
        return assessment;
    }

    private static CityClimateDataModel? FindCity(Dictionary<string, CityClimateDataModel> cities, string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return null;

        if (cities.TryGetValue(cityName.Trim(), out var city))
            return city;

        foreach (var kvp in cities)
        {
            if (cityName.Contains(kvp.Key, StringComparison.Ordinal) ||
                kvp.Key.Contains(cityName.Trim(), StringComparison.Ordinal))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    private DailyWeatherDto GenerateDailyWeather(CityClimateDataModel city, DateTime date)
    {
        var monthIndex = date.Month - 1;
        var avgTemp = city.AverageTempByMonth[monthIndex];
        var avgPrecip = city.PrecipitationByMonth[monthIndex];

        var tempVariation = _random.NextDouble() * 6 - 3;
        var temperature = avgTemp + tempVariation;

        var rainyDayChance = Math.Min(avgPrecip / 100.0, 0.8);
        var isRainy = _random.NextDouble() < rainyDayChance;
        double precipitation;

        if (isRainy)
        {
            var rainIntensity = _random.NextDouble();
            precipitation = rainIntensity switch
            {
                < 0.6 => _random.NextDouble() * 5 + 0.5,
                < 0.85 => _random.NextDouble() * 15 + 5,
                _ => _random.NextDouble() * 30 + 20
            };
        }
        else
        {
            precipitation = 0;
        }

        var baseHumidity = 50 + avgPrecip * 0.3;
        var humidityVariation = _random.NextDouble() * 20 - 10;
        var humidity = Math.Clamp(baseHumidity + humidityVariation, 20, 100);

        string condition;
        if (precipitation >= 20)
            condition = "大雨";
        else if (precipitation >= 10)
            condition = "中雨";
        else if (precipitation >= 3)
            condition = "小雨";
        else if (temperature >= 35)
            condition = "高温晴热";
        else if (temperature <= 0)
            condition = "寒冷";
        else if (humidity >= 80)
            condition = "阴湿";
        else
            condition = "晴好";

        return new DailyWeatherDto
        {
            Date = date,
            TemperatureC = Math.Round(temperature, 1),
            PrecipitationMm = Math.Round(precipitation, 1),
            Humidity = Math.Round(humidity, 1),
            WeatherCondition = condition
        };
    }
}
