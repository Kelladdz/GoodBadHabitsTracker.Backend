using FluentValidation;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Insert
{
    public sealed class InsertCommandValidator<TEntity, TRequest> : AbstractValidator<InsertCommand<TEntity, TRequest>>
            where TEntity : class
            where TRequest : class
    {
        public InsertCommandValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            if(typeof(TRequest) == typeof(HabitRequest))
            {
                RuleFor(x => (x.Request as HabitRequest)!.Name)
               .NotNull().WithMessage("Name cannot be null")
               .NotEmpty().WithMessage("Name is required")
               .MinimumLength(3).WithMessage("Name should be at least 3 characters")
               .MaximumLength(50).WithMessage("Name should not exceed 50 characters");

                RuleFor(x => (x.Request as HabitRequest)!.IconPath)
                    .NotNull().WithMessage("Icon path cannot be null");

                RuleFor(x => (x.Request as HabitRequest)!.HabitType)
                    .NotNull().WithMessage("Habit type cannot be null")
                    .IsInEnum().WithMessage("Invalid habit type");

                RuleFor(x => (x.Request as HabitRequest)!.StartDate)
                    .NotNull().WithMessage("Start date cannot be null")
                    .Must(x => x >= DateOnly.FromDateTime(DateTime.Now))
                        .WithMessage("Start date should be today or later");

                RuleFor(x => (x.Request as HabitRequest)!.IsTimeBased)
                    .Null()
                        .When(x => (x.Request as HabitRequest)!.HabitType == HabitTypes.Quit)
                        .WithMessage("For breaking a habit, IsTimeBased should be null");

                RuleFor(x => (x.Request as HabitRequest)!.IsTimeBased)
                    .NotNull()
                        .When(x => (x.Request as HabitRequest)!.HabitType != HabitTypes.Quit)
                        .WithMessage("For good and limiting habit, IsTimeBased flag cannot be null");

                RuleFor(x => (x.Request as HabitRequest)!.Quantity)
                    .Null()
                        .When(x => (x.Request as HabitRequest)!.HabitType == HabitTypes.Quit)
                        .WithMessage("For breaking a habit, Quantity should be null");

                RuleFor(x => (x.Request as HabitRequest)!.Quantity)
                    .NotNull()
                        .When(x => (x.Request as HabitRequest)!.HabitType != HabitTypes.Quit)
                        .WithMessage("For good and limiting habit Quantity cannot be null");

                RuleFor(x => (x.Request as HabitRequest)!.Frequency)
                    .Null()
                        .When(x => (x.Request as HabitRequest)!.HabitType == HabitTypes.Quit)
                        .WithMessage("For breaking a habit, Frequency should be null");

                RuleFor(x => (x.Request as HabitRequest)!.Frequency)
                   .NotNull()
                        .When(x => (x.Request as HabitRequest)!.HabitType != HabitTypes.Quit)
                        .WithMessage("For good and limiting habit, Frequency cannot be null");

                RuleFor(x => (x.Request as HabitRequest)!.Frequency)
                    .IsInEnum().WithMessage("Invalid Frequency");

                RuleFor(x => (x.Request as HabitRequest)!.RepeatMode)
                    .Null()
                        .When(x => (x.Request as HabitRequest)!.HabitType != HabitTypes.Good)
                        .WithMessage("For breaking or limiting a habit, RepeatMode should be null");

                RuleFor(x => (x.Request as HabitRequest)!.RepeatMode)
                    .NotNull()
                        .When(x => (x.Request as HabitRequest)!.HabitType == HabitTypes.Good)
                        .WithMessage("For good habit, RepeatMode cannot be null");

                RuleFor(x => (x.Request as HabitRequest)!.RepeatMode)
                    .IsInEnum().WithMessage("Invalid RepeatMode");

                RuleFor(x => (x.Request as HabitRequest)!.RepeatDaysOfWeek)
                    .Custom((value, context) =>
                    {
                        if ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Daily && value != null)
                        {
                            foreach (var day in (context.InstanceToValidate.Request as HabitRequest)!.RepeatDaysOfWeek!)
                            {
                                if (day is < 0 || day is > (DayOfWeek)6)
                                    context.AddFailure($"Invalid day of week");
                            }
                        }

                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode != RepeatModes.Daily && value != null)
                            context.AddFailure("If habit isn't in daily repeat mode, then RepeatDaysOfWeek should be null");

                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Daily && value == null)
                            context.AddFailure("If habit is in daily repeat mode, then RepeatDaysOfWeek cannot be null");
                    });

                RuleFor(x => (x.Request as HabitRequest)!.RepeatDaysOfMonth)
                    .Custom((value, context) =>
                    {
                        if ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Monthly && value != null)
                        {
                            foreach (var day in (context.InstanceToValidate.Request as HabitRequest)!.RepeatDaysOfMonth!)
                            {
                                if (day < 1 || day > 31)
                                    context.AddFailure($"Invalid day of month: {day}");
                            }
                        }
                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode != RepeatModes.Monthly && value != null)
                            context.AddFailure("If habit isn't in monthly repeat mode, then RepeatDaysOfMonth should be null");

                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Monthly && value == null)
                            context.AddFailure("If habit is in monthly repeat mode, then RepeatDaysOfMonth cannot be null");
                    });

                RuleFor(x => (x.Request as HabitRequest)!.RepeatInterval)
                    .Custom((value, context) =>
                    {
                        if ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Interval && (value > 1 || value < 8))
                            context.AddFailure("Interval should be greater than 1 and less than 8");

                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode != RepeatModes.Interval && (value > 1 || value < 8))
                            context.AddFailure("If habit isn't in interval repeat mode, then RepeatInterval should be null");

                        else if
                        ((context.InstanceToValidate.Request as HabitRequest)!.RepeatMode == RepeatModes.Interval && value == null)
                            context.AddFailure("If habit is in interval repeat mode, then RepeatInterval cannot be null");
                    });

                RuleFor(x => (x.Request as HabitRequest)!.ReminderTimes)
                    .Custom((value, context) =>
                    {
                        if ((context.InstanceToValidate.Request as HabitRequest)!.HabitType != HabitTypes.Good && value != null)
                            context.AddFailure("For breaking or limiting habit, ReminderTimes should be null");
                    });
            }
           
            else if(typeof(TRequest) == typeof(GroupRequest))
            {
                RuleFor(x => (x.Request as GroupRequest)!.Name)
                    .NotNull().WithMessage("Name cannot be null")
                    .NotEmpty().WithMessage("Name is required")
                    .MinimumLength(3).WithMessage("Name should be at least 3 characters")
                    .MaximumLength(15).WithMessage("Name should not exceed 15 characters");
            }
        }
    }
}
