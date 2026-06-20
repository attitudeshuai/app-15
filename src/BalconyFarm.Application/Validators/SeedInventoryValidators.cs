using BalconyFarm.Application.DTOs;
using FluentValidation;

namespace BalconyFarm.Application.Validators;

public class CreateSeedInventoryRequestDtoValidator : AbstractValidator<CreateSeedInventoryRequestDto>
{
    public CreateSeedInventoryRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("种子名称不能为空")
            .MaximumLength(100).WithMessage("种子名称长度不能超过100个字符");

        RuleFor(x => x.Variety)
            .NotEmpty().WithMessage("种子品种不能为空")
            .MaximumLength(100).WithMessage("种子品种长度不能超过100个字符");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("数量必须大于0");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("单位不能为空")
            .MaximumLength(50).WithMessage("单位长度不能超过50个字符");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("购买日期不能为空")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("购买日期不能晚于当前日期");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty().WithMessage("保质期不能为空")
            .GreaterThan(x => x.PurchaseDate).WithMessage("保质期必须晚于购买日期");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("备注长度不能超过1000个字符");
    }
}

public class UpdateSeedInventoryRequestDtoValidator : AbstractValidator<UpdateSeedInventoryRequestDto>
{
    public UpdateSeedInventoryRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("种子名称长度不能超过100个字符");

        RuleFor(x => x.Variety)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Variety))
            .WithMessage("种子品种长度不能超过100个字符");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).When(x => x.Quantity.HasValue)
            .WithMessage("数量必须大于0");

        RuleFor(x => x.Unit)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Unit))
            .WithMessage("单位长度不能超过50个字符");

        RuleFor(x => x.PurchaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.PurchaseDate.HasValue)
            .WithMessage("购买日期不能晚于当前日期");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("备注长度不能超过1000个字符");
    }
}

public class UseSeedRequestDtoValidator : AbstractValidator<UseSeedRequestDto>
{
    public UseSeedRequestDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("使用数量必须大于0");

        RuleFor(x => x.Note)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Note))
            .WithMessage("备注长度不能超过500个字符");
    }
}
