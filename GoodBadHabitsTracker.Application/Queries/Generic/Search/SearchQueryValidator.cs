using FluentValidation;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    public class SearchQueryValidator<TEntity> : AbstractValidator<SearchQuery<TEntity>>
        where TEntity : class
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.Date)
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

