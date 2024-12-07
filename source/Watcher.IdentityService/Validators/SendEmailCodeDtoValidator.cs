using FluentValidation;
using Watcher.IdentityService.DataObjects.Requests;

namespace Watcher.IdentityService.Validators;

public class SendEmailCodeDtoValidator : AbstractValidator<SendEmailCodeDto>
{
    public SendEmailCodeDtoValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(64);
    }
}