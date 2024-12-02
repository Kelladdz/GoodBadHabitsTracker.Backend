using FluentValidation;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Create
{
    internal sealed class CreateCommandValidator : AbstractValidator<CreateCommand>
    {
        public CreateCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                                .NotNull().WithMessage("Name cannot be null")
                                .NotEmpty().WithMessage("Name is required")
                                .MinimumLength(3).WithMessage("Name should be at least 3 characters")
                                .MaximumLength(15).WithMessage("Name should not exceed 15 characters");
        }
    }
}
