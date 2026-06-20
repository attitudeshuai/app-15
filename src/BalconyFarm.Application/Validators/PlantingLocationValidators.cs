using BalconyFarm.Application.DTOs;
using FluentValidation;

namespace BalconyFarm.Application.Validators;

public class CreatePlantingLocationRequestDtoValidator : AbstractValidator<CreatePlantingLocationRequestDto>
{
    public CreatePlantingLocationRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("位置名称不能为空")
            .MaximumLength(100).WithMessage("位置名称长度不能超过100个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("位置描述长度不能超过500个字符");

        RuleFor(x => x.LocationType)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.LocationType))
            .WithMessage("位置类型长度不能超过50个字符");

        RuleFor(x => x.SunlightCondition)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.SunlightCondition))
            .WithMessage("光照条件长度不能超过50个字符");

        RuleFor(x => x.Area)
            .GreaterThan(0).When(x => x.Area.HasValue)
            .WithMessage("面积必须大于0");
    }
}

public class UpdatePlantingLocationRequestDtoValidator : AbstractValidator<UpdatePlantingLocationRequestDto>
{
    public UpdatePlantingLocationRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("位置名称长度不能超过100个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null)
            .WithMessage("位置描述长度不能超过500个字符");

        RuleFor(x => x.LocationType)
            .MaximumLength(50).When(x => x.LocationType != null)
            .WithMessage("位置类型长度不能超过50个字符");

        RuleFor(x => x.SunlightCondition)
            .MaximumLength(50).When(x => x.SunlightCondition != null)
            .WithMessage("光照条件长度不能超过50个字符");

        RuleFor(x => x.Area)
            .GreaterThan(0).When(x => x.Area.HasValue)
            .WithMessage("面积必须大于0");
    }
}
