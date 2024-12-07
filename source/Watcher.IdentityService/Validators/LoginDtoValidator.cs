using FluentValidation;
using Watcher.IdentityService.DataObjects.Requests;

namespace Watcher.IdentityService.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(64);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("^[a-zA-Z0-9!@#$%^&*()]*$")
            .WithMessage("Password must contain only Latin letters, digits and special symbols.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}