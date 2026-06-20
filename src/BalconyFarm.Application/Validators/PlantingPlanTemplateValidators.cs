using BalconyFarm.Application.DTOs;
using FluentValidation;

namespace BalconyFarm.Application.Validators;

public class ApplyTemplateRequestDtoValidator : AbstractValidator<ApplyTemplateRequestDto>
{
    public ApplyTemplateRequestDtoValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("模板ID不能为空")
            .MaximumLength(100).WithMessage("模板ID长度不能超过100个字符");

        RuleFor(x => x.CropId)
            .NotEmpty().WithMessage("作物ID不能为空");

        RuleFor(x => x.PlantingDate)
            .NotEmpty().WithMessage("种植日期不能为空")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("种植日期不能晚于当前日期");
    }
}

public class PlantingPlanTemplateQueryRequestDtoValidator : AbstractValidator<PlantingPlanTemplateQueryRequestDto>
{
    public PlantingPlanTemplateQueryRequestDtoValidator()
    {
        RuleFor(x => x.Keyword)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Keyword))
            .WithMessage("搜索关键词长度不能超过100个字符");

        RuleFor(x => x.Difficulty)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Difficulty))
            .WithMessage("难度筛选长度不能超过20个字符");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("页码必须大于0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("每页数量必须大于0")
            .LessThanOrEqualTo(100).WithMessage("每页数量不能超过100");
    }
}
