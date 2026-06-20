using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface IPlantingPlanTemplateService
{
    Task<ApiResponse<PagedResult<PlantingPlanTemplateDto>>> GetAllTemplatesAsync(PlantingPlanTemplateQueryRequestDto query, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingPlanTemplateDto>> GetTemplateByIdAsync(string templateId, CancellationToken cancellationToken = default);
    Task<ApiResponse<PlantingPlanTemplateDto>> GetTemplateByCropNameAsync(string cropName, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<PlantingPlanTemplateDto>>> SearchTemplatesAsync(PlantingPlanTemplateQueryRequestDto query, CancellationToken cancellationToken = default);
    Task<ApiResponse<ApplyTemplateResultDto>> ApplyTemplateAsync(ApplyTemplateRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
}
