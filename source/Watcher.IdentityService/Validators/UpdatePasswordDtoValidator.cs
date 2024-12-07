using FluentValidation;
using Watcher.IdentityService.DataObjects.Requests;

namespace Watcher.IdentityService.Validators;

public class UpdatePasswordDtoValidator : AbstractValidator<UpdatePasswordDto>
{
    public UpdatePasswordDtoValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty()
            .Must(id => id != Guid.Empty).WithMessage("Id must be a valid GUID.");
    }
}