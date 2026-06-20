using BalconyFarm.Application.DTOs;
using FluentValidation;

namespace BalconyFarm.Application.Validators;

public class CreateCropPhotoRequestDtoValidator : AbstractValidator<CreateCropPhotoRequestDto>
{
    public CreateCropPhotoRequestDtoValidator()
    {
        RuleFor(x => x.CropId)
            .NotEmpty().WithMessage("作物ID不能为空");

        RuleFor(x => x.PhotoDate)
            .NotEmpty().WithMessage("拍摄日期不能为空")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("拍摄日期不能晚于当前日期");

        RuleFor(x => x.PhotoUrl)
            .NotEmpty().WithMessage("照片URL不能为空")
            .MaximumLength(500).WithMessage("照片URL长度不能超过500个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("描述长度不能超过500个字符");
    }
}

public class UpdateCropPhotoRequestDtoValidator : AbstractValidator<UpdateCropPhotoRequestDto>
{
    public UpdateCropPhotoRequestDtoValidator()
    {
        RuleFor(x => x.PhotoDate)
            .LessThanOrEqualTo(DateTime.UtcNow).When(x => x.PhotoDate.HasValue)
            .WithMessage("拍摄日期不能晚于当前日期");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.PhotoUrl))
            .WithMessage("照片URL长度不能超过500个字符");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("描述长度不能超过500个字符");
    }
}
