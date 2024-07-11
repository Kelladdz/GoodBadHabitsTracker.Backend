using FluentValidation;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Create
{
    public sealed class CreateQuitHabitCommandValidator : AbstractValidator<CreateQuitHabitCommand>
    {
        public CreateQuitHabitCommandValidator()
        {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            RuleFor(x => x.Request.IsGood)
                .Must(x => x == false).WithMessage("For breaking habit IsGood should be false.");
            RuleFor(x => x.Request.IsQuit)
                .Must(x => x == true).WithMessage("For breaking habit IsQuit should be true.");
            RuleFor(x => x.Request.Frequency)
                .Null().WithMessage("For breaking habit frequency should be null.");
            RuleFor(x => x.Request.IsTimeBased)
                .Null().WithMessage("For breaking habit, IsTimeBased should be null.");
            RuleFor(x => x.Request.Quantity)
                .Null().WithMessage("For breaking habit, quantity should be null.");
            RuleFor(x => x.Request.RepeatMode)
                .Null().WithMessage("For breaking habit, repeat mode should be null.");
            RuleFor(x => x.Request.RepeatDaysOfMonth)
                .Empty().WithMessage("For breaking habit, list of repeat days should be empty.");
            RuleFor(x => x.Request.RepeatDaysOfWeek)
                .Empty().WithMessage("For breaking habit, list of repeat days should be empty.");
            RuleFor(x => x.Request.RepeatInterval)
                .Empty().WithMessage("For breaking habit, repeat interval value should be 0.");
            RuleFor(x => x.Request.StartDate)
                .Must(x => x >= DateOnly.FromDateTime(DateTime.Now)).WithMessage("Start date must be today or later.");
            RuleFor(x => x.Request.ReminderTimes)
                .Empty().WithMessage("For breaking habit, list of reminder times should be empty.");
        }
    }
}
