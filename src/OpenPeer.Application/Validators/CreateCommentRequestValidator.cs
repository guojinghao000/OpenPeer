using FluentValidation;
using OpenPeer.Application.DTOs.Comments;

namespace OpenPeer.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("评论内容不能为空")
            .MinimumLength(1).WithMessage("评论最少1个字符")
            .MaximumLength(5000).WithMessage("评论最多5000个字符");
    }
}
