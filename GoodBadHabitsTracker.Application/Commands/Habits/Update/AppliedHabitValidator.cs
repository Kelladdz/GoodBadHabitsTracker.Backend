using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    internal sealed class AppliedHabitValidator : AbstractValidator<Habit>
    {
        public AppliedHabitValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
               .NotNull().WithMessage("Name cannot be null")
               .NotEmpty().WithMessage("Name is required")
               .MinimumLength(3).WithMessage("Name should be at least 3 characters")
               .MaximumLength(50).WithMessage("Name should not exceed 50 characters");

            RuleFor(x => x.IconId)
                .NotNull().WithMessage("Icon path cannot be null");

            RuleFor(x => x.HabitType)
                .NotNull().WithMessage("Habit type cannot be null")
                .IsInEnum().WithMessage("Invalid habit type");

            RuleFor(x => x.IsTimeBased)
                .Null()
                    .When(x => x.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, IsTimeBased should be null");

            RuleFor(x => x.IsTimeBased)
                .NotNull()
                    .When(x => x.HabitType != HabitTypes.Quit)
                    .WithMessage("For good and limiting habit, IsTimeBased flag cannot be null");

            RuleFor(x => x.Quantity)
                .Null()
                    .When(x => x.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, Quantity should be null");

            RuleFor(x => x.Quantity)
                .NotNull()
                    .When(x => x.HabitType != HabitTypes.Quit)
                    .WithMessage("For good and limiting habit Quantity cannot be null");

            RuleFor(x => x.Frequency)
                .Must(x => x == Frequencies.NonApplicable)
                    .When(x => x.HabitType == HabitTypes.Quit)
                    .WithMessage("For breaking a habit, Frequency shouldn't be applicable");

            RuleFor(x => x.Frequency)
               .NotNull() .WithMessage("Frequency cannot be null");

            RuleFor(x => x.Frequency)
                .IsInEnum().WithMessage("Invalid Frequency");

            RuleFor(x => x.RepeatMode)
                .Must(x => x == RepeatModes.NonApplicable)
                    .When(x => x.HabitType != HabitTypes.Good)
                    .WithMessage("For breaking or limiting a habit, RepeatMode shouldn't be applicable");

            RuleFor(x => x.RepeatMode)
                .NotNull().WithMessage("RepeatMode cannot be null");

            RuleFor(x => x.RepeatMode)
                .NotNull()
                    .When(x => x.HabitType == HabitTypes.Good)
                    .WithMessage("For good habit, RepeatMode cannot be null");

            RuleFor(x => x.RepeatMode)
                .IsInEnum().WithMessage("Invalid RepeatMode");

            RuleFor(x => x.RepeatDaysOfWeek)
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.RepeatMode == RepeatModes.Daily && value is not null))
                    {
                        foreach (var day in (context.InstanceToValidate.RepeatDaysOfWeek!))
                        {
                            if (day is < 0 || day is > (DayOfWeek)6)
                                context.AddFailure($"Invalid day of week");
                        }
                    }

                    else if
                    ((context.InstanceToValidate.RepeatMode != RepeatModes.Daily && (value is not null && value.Count != 0)))
                        context.AddFailure("If habit isn't in daily repeat mode, then RepeatDaysOfWeek should be null");

                    else if
                    ((context.InstanceToValidate.RepeatMode == RepeatModes.Daily && (value is null && value.Count == 0)))
                        context.AddFailure("If habit is in daily repeat mode, then RepeatDaysOfWeek cannot be null");

                    else if ((context.InstanceToValidate.RepeatMode == RepeatModes.NonApplicable && value.Count != 0))
                        context.AddFailure("If habit is in non applicable repeat mode, then RepeatDaysOfWeek should be null");
                });

            RuleFor(x => (x.RepeatDaysOfMonth))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.RepeatMode == RepeatModes.Monthly && value is not null))
                    {
                        foreach (var day in (context.InstanceToValidate.RepeatDaysOfMonth!))
                        {
                            if (day < 1 || day > 31)
                                context.AddFailure($"Invalid day of month: {day}");
                        }
                    }
                    else if
                    ((context.InstanceToValidate.RepeatMode != RepeatModes.Monthly && value is not null && value.Count != 0))
                        context.AddFailure("If habit isn't in monthly repeat mode, then RepeatDaysOfMonth should be null");

                    else if
                    ((context.InstanceToValidate.RepeatMode == RepeatModes.Monthly && (value is null && value.Count == 0)))
                        context.AddFailure("If habit is in monthly repeat mode, then RepeatDaysOfMonth cannot be null");

                    else if ((context.InstanceToValidate.RepeatMode == RepeatModes.NonApplicable && value.Count != 0))
                        context.AddFailure("If habit is in non applicable repeat mode, then RepeatDaysOfWeek should be null");
                });

            RuleFor(x => (x.RepeatInterval))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.RepeatMode == RepeatModes.Interval && (value < 2 && value > 7)))
                        context.AddFailure("Interval should be greater than 1 and less than 8");

                    else if
                    ((context.InstanceToValidate.RepeatMode != RepeatModes.Interval && value != 0))
                        context.AddFailure("If habit isn't in interval repeat mode, then RepeatInterval should be null");

                    else if
                    ((context.InstanceToValidate.RepeatMode == RepeatModes.Interval && value == 0))
                        context.AddFailure("If habit is in interval repeat mode, then RepeatInterval cannot be null");

                    else if ((context.InstanceToValidate.RepeatMode == RepeatModes.NonApplicable && value != 0))
                        context.AddFailure("If habit is in non applicable repeat mode, then RepeatDaysOfWeek should be null");
                });

            /*RuleFor(x => (x.ReminderTimes))
                .Custom((value, context) =>
                {
                    if ((context.InstanceToValidate.HabitType != HabitTypes.Good && value is not null))
                        context.AddFailure("For breaking or limiting habit, ReminderTimes should be null");
                });*/
        }
    }
}
