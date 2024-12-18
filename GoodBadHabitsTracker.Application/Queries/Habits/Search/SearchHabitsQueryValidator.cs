using FluentValidation;

namespace GoodBadHabitsTracker.Application.Queries.Habits.Search
{
    public class SearchHabitsQueryValidator : AbstractValidator<SearchHabitsQuery>
    {
        public SearchHabitsQueryValidator()
        {
            RuleFor(x => x.Date)
                   .Must(BeEarlierThenDayAfterTommorrow).WithMessage("The date must be within the next 2 days.");
        }

        private bool BeEarlierThenDayAfterTommorrow(DateOnly date)
        {
            var today = DateTime.Today;
            var twoDaysFromNow = DateOnly.FromDateTime(today.AddDays(2));
            return date < twoDaysFromNow;
        }
    }
}
