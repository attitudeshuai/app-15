using System.Reflection;
using System.Text.Json;

namespace BalconyFarm.Application.Data;

public class JsonPlantingPlanTemplateDataProvider : IPlantingPlanTemplateDataProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectory;

    public JsonPlantingPlanTemplateDataProvider()
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        _dataDirectory = Path.Combine(assemblyLocation, "Data");

        if (!Directory.Exists(_dataDirectory))
        {
            var projectRoot = GetProjectRoot();
            if (projectRoot != null)
            {
                _dataDirectory = Path.Combine(projectRoot, "Data");
            }
        }
    }

    private static string? GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.GetDirectories(currentDir).Any(d => d.EndsWith("BalconyFarm.Application")))
            {
                var appDir = Path.Combine(currentDir, "BalconyFarm.Application");
                if (Directory.Exists(Path.Combine(appDir, "Data")))
                {
                    return appDir;
                }
            }
            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }
        return null;
    }

    public async Task<List<PlantingPlanTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await LoadJsonAsync<List<PlantingPlanTemplate>>("planting-plan-templates.json", cancellationToken) ?? new();
    }

    public async Task<PlantingPlanTemplate?> GetTemplateByIdAsync(string templateId, CancellationToken cancellationToken = default)
    {
        var templates = await GetAllTemplatesAsync(cancellationToken);
        return templates.FirstOrDefault(t => t.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PlantingPlanTemplate?> GetTemplateByCropNameAsync(string cropName, CancellationToken cancellationToken = default)
    {
        var templates = await GetAllTemplatesAsync(cancellationToken);
        return templates.FirstOrDefault(t =>
            t.CropName.Equals(cropName, StringComparison.OrdinalIgnoreCase) ||
            t.Aliases.Any(a => a.Equals(cropName, StringComparison.OrdinalIgnoreCase)) ||
            t.CropName.Contains(cropName, StringComparison.OrdinalIgnoreCase) ||
            t.Aliases.Any(a => a.Contains(cropName, StringComparison.OrdinalIgnoreCase)) ||
            cropName.Contains(t.CropName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<PlantingPlanTemplate>> SearchTemplatesAsync(string keyword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return await GetAllTemplatesAsync(cancellationToken);
        }

        var templates = await GetAllTemplatesAsync(cancellationToken);
        return templates.Where(t =>
            t.CropName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            t.Variety.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            t.Aliases.Any(a => a.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
            t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            (t.Tips != null && t.Tips.Contains(keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    private async Task<T?> LoadJsonAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(_dataDirectory, fileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"数据文件不存在: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
