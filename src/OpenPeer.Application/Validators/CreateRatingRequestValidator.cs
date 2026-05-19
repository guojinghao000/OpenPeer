using FluentValidation;
using OpenPeer.Application.DTOs.Ratings;

namespace OpenPeer.Application.Validators;

public class CreateRatingRequestValidator : AbstractValidator<CreateRatingRequest>
{
    public CreateRatingRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("评分必须在1到5之间");
    }
}
