using FluentValidation;
using OpenPeer.Application.DTOs.Users;

namespace OpenPeer.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("请输入当前密码");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("请输入新密码")
            .MinimumLength(8).WithMessage("密码最少8个字符")
            .MaximumLength(100).WithMessage("密码最多100个字符")
            .Matches("[a-zA-Z]").WithMessage("密码必须包含字母")
            .Matches("[0-9]").WithMessage("密码必须包含数字");
        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("两次密码输入不一致");
    }
}
