using FluentValidation;
using Watcher.IdentityService.DataObjects.Requests;

namespace Watcher.IdentityService.Validators;

public class ConfirmCodeDtoValidator : AbstractValidator<ConfirmCodeDto>
{
    public ConfirmCodeDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(64);
        RuleFor(x => x.Code).NotEmpty();
    }
}