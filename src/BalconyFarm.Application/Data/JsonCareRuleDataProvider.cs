using System.Reflection;
using System.Text.Json;

namespace BalconyFarm.Application.Data;

public class JsonCareRuleDataProvider : ICareRuleDataProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _dataDirectory;

    public JsonCareRuleDataProvider()
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

    public async Task<List<CropCareRule>> GetCareRulesAsync(CancellationToken cancellationToken = default)
    {
        return await LoadJsonAsync<List<CropCareRule>>("care-rules.json", cancellationToken) ?? new();
    }

    public async Task<CropCareRule?> GetCareRuleByCropNameAsync(string cropName, CancellationToken cancellationToken = default)
    {
        var rules = await GetCareRulesAsync(cancellationToken);
        return rules.FirstOrDefault(r =>
            r.CropName.Equals(cropName, StringComparison.OrdinalIgnoreCase) ||
            r.Aliases.Any(a => a.Equals(cropName, StringComparison.OrdinalIgnoreCase)) ||
            r.CropName.Contains(cropName, StringComparison.OrdinalIgnoreCase) ||
            r.Aliases.Any(a => a.Contains(cropName, StringComparison.OrdinalIgnoreCase)) ||
            cropName.Contains(r.CropName, StringComparison.OrdinalIgnoreCase));
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
