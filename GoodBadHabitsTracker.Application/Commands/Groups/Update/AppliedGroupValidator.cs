using FluentValidation;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    internal sealed class AppliedGroupValidator : AbstractValidator<Group>
    {
        public AppliedGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotNull().WithMessage("Name cannot be null").WithErrorCode("NullName")
                .NotEmpty().WithMessage("Name is required").WithErrorCode("EmptyName")
                .MinimumLength(3).WithMessage("Name should be at least 3 characters").WithErrorCode("ShortName")
                .MaximumLength(15).WithMessage("Name should not exceed 15 characters").WithErrorCode("LongName");
        }
    }
}
