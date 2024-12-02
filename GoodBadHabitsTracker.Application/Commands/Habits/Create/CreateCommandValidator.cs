using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    internal sealed class CreateCommandValidator : AbstractValidator<CreateCommand>
    {
        public CreateCommandValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Request.Name)
               .NotNull().WithMessage("Name cannot be null")
               .NotEmpty().WithMessage("Name is required")
               .MinimumLength(3).WithMessage("Name should be at least 3 characters")
               .MaximumLength(50).WithMessage("Name should not exceed 50 characters");

            RuleFor(x => x.Request.IconId)
                .NotNull().WithMessage("Icon path cannot be null");

            RuleFor(x => x.Request.HabitType)
                .NotNull().WithMessage("Habit type cannot be null")
                .IsInEnum().WithMessage("Invalid habit type");

            RuleFor(x => x.Request.StartDate)
                .NotNull().WithMessage("Start date cannot be null")
                .Must(x => x >= DateOnly.FromDateTime(DateTime.Now))
                    .WithMessage("Start date should be today or later");

            RuleFor(x => x.Request.IsTimeBased)
                .Null()
                    .When(x => x.Request.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, IsTimeBased should be null");

            RuleFor(x => x.Request.IsTimeBased)
                .NotNull()
                    .When(x => x.Request.HabitType != HabitTypes.Quit)
                    .WithMessage("For good and limiting habit, IsTimeBased flag cannot be null");

            RuleFor(x => x.Request.Quantity)
                .Null()
                    .When(x => x.Request.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, Quantity should be null");

            RuleFor(x => x.Request.Quantity)
                .NotNull()
                    .When(x => x.Request.HabitType != HabitTypes.Quit)
                    .WithMessage("For good and limiting habit Quantity cannot be null");

            RuleFor(x => x.Request.Frequency)
                .Null()
                    .When(x => x.Request.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, Frequency should be null");

            RuleFor(x => x.Request.Frequency)
               .NotNull()
                    .When(x => x.Request.HabitType != HabitTypes.Quit)
                    .WithMessage("For good and limiting habit, Frequency cannot be null");

            RuleFor(x => x.Request.Frequency)
                .IsInEnum().WithMessage("Invalid Frequency");

            RuleFor(x => x.Request.RepeatMode)
                .Null()
                    .When(x => x.Request.HabitType != HabitTypes.Good)
                    .WithMessage("For breaking or limiting a habit, RepeatMode should be null");

            RuleFor(x => x.Request.RepeatMode)
                .NotNull()
                    .When(x => x.Request.HabitType == HabitTypes.Good)
                    .WithMessage("For good habit, RepeatMode cannot be null");

            RuleFor(x => x.Request.RepeatMode)
                .IsInEnum().WithMessage("Invalid RepeatMode");

            RuleFor(x => x.Request.RepeatDaysOfWeek)
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Daily && value is not null))
                    {
                        foreach (var day in (context.InstanceToValidate.Request.RepeatDaysOfWeek!))
                        {
                            if (day is < 0 || day is > (DayOfWeek)6)
                                context.AddFailure($"Invalid day of week");
                        }
                    }

                    else if
                    ((context.InstanceToValidate.Request.RepeatMode != RepeatModes.Daily && value is not null))
                        context.AddFailure("If habit isn't in daily repeat mode, then RepeatDaysOfWeek should be null");

                    else if
                    ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Daily && value is null))
                        context.AddFailure("If habit is in daily repeat mode, then RepeatDaysOfWeek cannot be null");
                });

            RuleFor(x => (x.Request.RepeatDaysOfMonth))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Monthly && value is not null))
                    {
                        foreach (var day in (context.InstanceToValidate.Request.RepeatDaysOfMonth!))
                        {
                            if (day < 1 || day > 31)
                                context.AddFailure($"Invalid day of month: {day}");
                        }
                    }
                    else if
                    ((context.InstanceToValidate.Request.RepeatMode != RepeatModes.Monthly && value is not null))
                        context.AddFailure("If habit isn't in monthly repeat mode, then RepeatDaysOfMonth should be null");

                    else if
                    ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Monthly && value is null))
                        context.AddFailure("If habit is in monthly repeat mode, then RepeatDaysOfMonth cannot be null");
                });

            RuleFor(x => (x.Request.RepeatInterval))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Interval && (value < 1 || value > 8)))
                        context.AddFailure("Interval should be greater than 1 and less than 8");

                    else if
                    ((context.InstanceToValidate.Request.RepeatMode != RepeatModes.Interval && value is not null))
                        context.AddFailure("If habit isn't in interval repeat mode, then RepeatInterval should be null");

                    else if
                    ((context.InstanceToValidate.Request.RepeatMode == RepeatModes.Interval && value is null))
                        context.AddFailure("If habit is in interval repeat mode, then RepeatInterval cannot be null");
                });

            RuleFor(x => (x.Request.ReminderTimes))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.Request.HabitType != HabitTypes.Good && value is not null))
                        context.AddFailure("For breaking or limiting habit, ReminderTimes should be null");
                });
        }
    }
}
