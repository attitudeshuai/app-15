using BalconyFarm.Application.DTOs;
using FluentValidation;

namespace BalconyFarm.Application.Validators;

public class CreateQuestionRequestDtoValidator : AbstractValidator<CreateQuestionRequestDto>
{
    public CreateQuestionRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("问题标题不能为空")
            .MaximumLength(200).WithMessage("问题标题长度不能超过200个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("问题内容不能为空")
            .MaximumLength(5000).WithMessage("问题内容长度不能超过5000个字符");

        RuleFor(x => x.CropType)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.CropType))
            .WithMessage("作物类型长度不能超过50个字符");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 5)
            .WithMessage("标签数量不能超过5个");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("标签名称不能为空")
            .MaximumLength(20).WithMessage("单个标签长度不能超过20个字符");
    }
}

public class UpdateQuestionRequestDtoValidator : AbstractValidator<UpdateQuestionRequestDto>
{
    public UpdateQuestionRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("问题标题长度不能超过200个字符");

        RuleFor(x => x.Content)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("问题内容长度不能超过5000个字符");

        RuleFor(x => x.CropType)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.CropType))
            .WithMessage("作物类型长度不能超过50个字符");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 5)
            .WithMessage("标签数量不能超过5个");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("标签名称不能为空")
            .MaximumLength(20).WithMessage("单个标签长度不能超过20个字符");
    }
}

public class CreateReplyRequestDtoValidator : AbstractValidator<CreateReplyRequestDto>
{
    public CreateReplyRequestDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("回复内容不能为空")
            .MaximumLength(2000).WithMessage("回复内容长度不能超过2000个字符");
    }
}

public class UpdateReplyRequestDtoValidator : AbstractValidator<UpdateReplyRequestDto>
{
    public UpdateReplyRequestDtoValidator()
    {
        RuleFor(x => x.Content)
            .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("回复内容长度不能超过2000个字符");
    }
}
