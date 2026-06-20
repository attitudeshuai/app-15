namespace BalconyFarm.Application.Data;

public interface IPlantingPlanTemplateDataProvider
{
    Task<List<PlantingPlanTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<PlantingPlanTemplate?> GetTemplateByIdAsync(string templateId, CancellationToken cancellationToken = default);
    Task<PlantingPlanTemplate?> GetTemplateByCropNameAsync(string cropName, CancellationToken cancellationToken = default);
    Task<List<PlantingPlanTemplate>> SearchTemplatesAsync(string keyword, CancellationToken cancellationToken = default);
}
