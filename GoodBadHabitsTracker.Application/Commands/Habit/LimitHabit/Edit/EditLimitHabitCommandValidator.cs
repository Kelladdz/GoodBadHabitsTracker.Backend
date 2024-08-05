using FluentValidation;
using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Edit
{
    public sealed class EditQuitHabitCommandValidator : AbstractValidator<EditLimitHabitCommand>
    {
        public EditQuitHabitCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            RuleFor(x => x.Request.IsGood)
                .Must(x => x == false).WithMessage("For limiting habit IsGood should be false.");
            RuleFor(x => x.Request.IsQuit)
                .Must(x => x == false).WithMessage("IsQuit should be false set for limiting habit.");
            RuleFor(x => x.Request.Frequency)
                .Must(x => x == Frequencies.PerDay || x == Frequencies.PerWeek || x == Frequencies.PerMonth).WithMessage("Invalid frequency");
            RuleFor(x => x.Request.Quantity)
                .NotNull().WithMessage("For limiting habit, quantity shouldn't be null.");
            RuleFor(x => x.Request.RepeatMode)
                .Null().WithMessage("For limiting habit, repeat mode should be null.");
            RuleFor(x => x.Request.RepeatDaysOfMonth)
                .Empty().WithMessage("For limiting habit, list of repeat days should be empty.");
            RuleFor(x => x.Request.RepeatDaysOfWeek)
                .Empty().WithMessage("For limiting habit, list of repeat days should be empty.");
            RuleFor(x => x.Request.RepeatInterval)
                .Must(x => x == 0).WithMessage("For limiting habit, repeat interval value should be 0.");
            RuleFor(x => x.Request.ReminderTimes)
                .Empty().WithMessage("For limiting habit, list of reminder times should be empty.");
        }
    }
}
