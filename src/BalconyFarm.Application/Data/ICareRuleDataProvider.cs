namespace BalconyFarm.Application.Data;

public interface ICareRuleDataProvider
{
    Task<List<CropCareRule>> GetCareRulesAsync(CancellationToken cancellationToken = default);
    Task<CropCareRule?> GetCareRuleByCropNameAsync(string cropName, CancellationToken cancellationToken = default);
}
