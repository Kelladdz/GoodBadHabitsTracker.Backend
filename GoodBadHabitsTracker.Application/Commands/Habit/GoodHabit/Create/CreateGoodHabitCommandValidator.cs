using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create
{
    public sealed class CreateGoodHabitCommandValidator : AbstractValidator<CreateGoodHabitCommand>
    {
        private readonly List<string> DaysOfWeek = ["monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday"];
        public CreateGoodHabitCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            RuleFor(x => x.Request.IsQuit)
                .Empty().When(x => x.Request.IsGood == true).WithMessage("IsQuit shouldn't be set if habit is good.");
            RuleFor(x => x.Request.Frequency)
                .Must(x => x == Frequencies.PerDay || x == Frequencies.PerWeek || x == Frequencies.PerMonth).When(x => x.Request.IsQuit == false).WithMessage("Invalid frequency")
                .Empty().When(x => x.Request.IsQuit == true).WithMessage("For breaking a habit, frequency should be empty.");
            RuleFor(x => x.Request.IsTimeBased)
                .Empty().When(x => x.Request.IsQuit == true).WithMessage("For breaking a habit, IsGoalInTime should be false");
            RuleFor(x => x.Request.Quantity)
                .Must(x => x < 256 && x != null).When(x => x.Request.IsQuit == false).WithMessage("Invalid quantity")
                .Empty().When(x => x.Request.IsQuit == true).WithMessage("For breaking a habit, quantity should be empty");
            RuleFor(x => x.Request.RepeatMode)
                .Must(x => x == RepeatModes.Daily || x == RepeatModes.Monthly || x == RepeatModes.Interval).When(x => x.Request.IsQuit == false).WithMessage("Repeat mode must be Daily, Weekly or Monthly.")
                .Empty().When(x => x.Request.IsQuit == true).WithMessage("For breaking a habit, repeat mode should be empty");
            RuleFor(x => x.Request.RepeatDaysOfMonth)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Monthly)
                    {
                        if (context.InstanceToValidate.Request.IsGood == true)
                        {
                            foreach (var day in context.InstanceToValidate.Request.RepeatDaysOfMonth)
                            {
                                if (day < 1 || day > 31)
                                    context.AddFailure($"Invalid day of month: {day}");
                            }
                        }
                        else context.AddFailure("For breaking a habit, repeat mode should be empty");
                    }
                    else
                    {
                        if (value is not null)
                            context.AddFailure("RepeatDaysOfMonth should be empty if repeat mode isn't 'Monthly'.");
                    }
                });
            RuleFor(x => x.Request.RepeatDaysOfWeek)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Daily)
                    {
                        if (context.InstanceToValidate.Request.IsGood == true)
                        {
                            foreach (var day in context.InstanceToValidate.Request.RepeatDaysOfWeek)
                            {
                                if (!DaysOfWeek.Contains(day))
                                    context.AddFailure($"Invalid day of week: {day}");
                            }
                        }
                        else context.AddFailure("For breaking a habit, repeat mode should be empty");
                    }
                    else
                    {
                        if (value is not null)
                            context.AddFailure("RepeatDaysOfWeek should be empty if repeat mode isn't 'Daily'.");
                    }
                });
            RuleFor(x => x.Request.RepeatInterval)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.RepeatMode == RepeatModes.Interval)
                    {
                        if (context.InstanceToValidate.Request.IsGood == true)
                        {
                            if (value < 1 || value > 8)
                                context.AddFailure("Repeat interval must be greater than 1 and less than 8.");
                        }
                        else context.AddFailure("For breaking a habit, repeat mode should be empty");
                    }
                    else
                    {
                        if (value != null)
                            context.AddFailure("RepeatInterval should be empty if repeat mode isn't 'Interval'.");
                    }
                });
            RuleFor(x => x.Request.StartDate)
                .Must(x => x >= DateOnly.FromDateTime(DateTime.Now)).WithMessage("Start date must be today or later.");
            RuleFor(x => x.Request.ReminderTimes)
                .Custom((value, context) =>
                {
                    if (context.InstanceToValidate.Request.IsGood == false)
                    {
                        if (value.Count() > 0)
                            context.AddFailure("ReminderTimes shouldn't be set if habit is good.");
                    }
                    else
                    {
                        foreach (var time in value)
                        {
                            if (time.Hour < 0 || time.Hour > 23 || time.Minute < 0 || time.Minute > 59)
                                context.AddFailure($"Invalid time: {time}");
                        }
                    }
                });
        }
    }
}

