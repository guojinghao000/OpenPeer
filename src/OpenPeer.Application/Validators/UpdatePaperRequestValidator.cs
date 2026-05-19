using FluentValidation;
using OpenPeer.Application.DTOs.Papers;

namespace OpenPeer.Application.Validators;

public class UpdatePaperRequestValidator : AbstractValidator<UpdatePaperRequest>
{
    public UpdatePaperRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .MinimumLength(5).WithMessage("标题最少5个字符")
            .MaximumLength(200).WithMessage("标题最多200个字符");

        RuleFor(x => x.Abstract)
            .NotEmpty().WithMessage("摘要不能为空")
            .MinimumLength(20).WithMessage("摘要最少20个字符")
            .MaximumLength(2000).WithMessage("摘要最多2000个字符");
    }
}
