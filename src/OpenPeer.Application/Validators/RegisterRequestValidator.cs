using FluentValidation;
using OpenPeer.Application.DTOs.Auth;

namespace OpenPeer.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名最少3个字符")
            .MaximumLength(20).WithMessage("用户名最多20个字符")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("用户名只能包含字母、数字和下划线");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("邮箱不能为空")
            .EmailAddress().WithMessage("邮箱格式不正确");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(8).WithMessage("密码最少8个字符")
            .MaximumLength(100).WithMessage("密码最多100个字符")
            .Matches("[a-zA-Z]").WithMessage("密码必须包含字母")
            .Matches("[0-9]").WithMessage("密码必须包含数字");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("两次密码输入不一致");
    }
}
