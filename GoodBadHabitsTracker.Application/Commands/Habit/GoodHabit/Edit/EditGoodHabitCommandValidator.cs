using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Edit
{
    public sealed class EditGoodHabitCommandValidator : AbstractValidator<EditGoodHabitCommand>
    {
        private readonly List<string> DaysOfWeek = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        public EditGoodHabitCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            RuleFor(x => x.Request.IsGood)
                .Must(x => x == true).WithMessage("For good habit IsGood should be true.");
            RuleFor(x => x.Request.IsQuit)
                .Null().WithMessage("IsQuit shouldn't be set if habit is good.");
            RuleFor(x => x.Request.Quantity)
                .NotNull().WithMessage("For good habit, quantity shouldn't be null");
            RuleFor(x => x.Request.Frequency)
                .Must(x => x == Frequencies.PerDay || x == Frequencies.PerWeek || x == Frequencies.PerMonth).WithMessage("Invalid frequency");
            RuleFor(x => x.Request.RepeatMode)
                .Must(x => x == RepeatModes.Daily || x == RepeatModes.Monthly || x == RepeatModes.Interval).WithMessage("Repeat mode must be Daily, Weekly or Monthly.")
                .NotNull().WithMessage("For good habit, repeat mode shouldn't be null.");
            RuleFor(x => x.Request.RepeatDaysOfMonth)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Monthly)
                    {
                        if (value.Length == 0)
                            context.AddFailure("RepeatDaysOfMonth shouldn't be empty if repeat mode is 'Monthly'.");
                        foreach (var day in context.InstanceToValidate.Request.RepeatDaysOfMonth)
                        {
                            if (day < 1 || day > 31)
                                context.AddFailure($"Invalid day of month: {day}");
                        }
                    }
                    else
                    {
                        if (value.Length != 0)
                            context.AddFailure("RepeatDaysOfMonth should be empty if repeat mode isn't 'Monthly'.");
                    }
                });
            RuleFor(x => x.Request.RepeatDaysOfWeek)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Daily)
                    {
                        if (value.Length == 0)
                            context.AddFailure("RepeatDaysOfWeek shouldn't be empty if repeat mode is 'Daily'.");
                        foreach (var day in context.InstanceToValidate.Request.RepeatDaysOfWeek)
                        {
                            if (!DaysOfWeek.Contains(day))
                                context.AddFailure($"Invalid day of week: {day}");
                        }
                    }
                    else
                    {
                        if (value.Length != 0)
                            context.AddFailure("RepeatDaysOfWeek should be empty if repeat mode isn't 'Daily'.");
                    }
                });
            RuleFor(x => x.Request.RepeatInterval)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Interval)
                    {
                        if (value < 1 || value > 8)
                            context.AddFailure("Repeat interval value must be greater than 1 and less than 8.");
                    }
                    else
                    {
                        if (value != 0)
                            context.AddFailure("Repeat interval value should be 0 if repeat mode isn't 'Interval'.");
                    }
                });
            RuleFor(x => x.Request.ReminderTimes)
                .Custom((value, context) =>
                {
                        foreach (var time in value)
                        {
                            if (time.Hour < 0 || time.Hour > 23 || time.Minute < 0 || time.Minute > 59)
                                context.AddFailure($"Invalid time: {time}");
                        }

                });
        }
    }
}

