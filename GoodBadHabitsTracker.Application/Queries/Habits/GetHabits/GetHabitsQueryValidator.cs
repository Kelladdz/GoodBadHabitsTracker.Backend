using FluentValidation;
using GoodBadHabitsTracker.Application.Queries.Habits.GetHabits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Queries.Habits.GetHabits
{
    public sealed class GetHabitsQueryValidator : AbstractValidator<GetHabitsQuery>
    {
        public GetHabitsQueryValidator()
        {
            RuleFor(x => x.Request.Date)
                .Must(BeWithinNextSevenDays).WithMessage("The date must be within the next 7 days.");
        }

        private bool BeWithinNextSevenDays(DateOnly date)
        {
            var today = DateTime.Today;
            var sevenDaysFromNow = DateOnly.FromDateTime(today.AddDays(7));
            return date <= sevenDaysFromNow;
        }

    }
}

