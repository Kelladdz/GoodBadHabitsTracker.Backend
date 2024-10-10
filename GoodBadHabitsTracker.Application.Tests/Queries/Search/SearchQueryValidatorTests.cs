using Azure.Core;
using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Application.Queries.Generic.Search;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Queries.Search
{
    public class SearchQueryValidatorTests
    {
        private readonly SearchQueryValidator<Habit> _habitsValidator;
        private readonly DataGenerator _dataGenerator;

        public SearchQueryValidatorTests()
        {
            _habitsValidator = new();
            _dataGenerator = new();
        }

        [Fact]
        public void Habit_ValidCommand_DoesntThrowException()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(10);
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new SearchQuery<Habit>(term, date);

            //ACT
            var result = _habitsValidator.TestValidate(new SearchQuery<Habit>(term, date));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(x => x.Date);
        }

        [Fact]
        public void Habit_InvalidDate_ThrowException()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(10);
            var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
            var command = new SearchQuery<Habit>(term, date);

            //ACT
            var result = _habitsValidator.TestValidate(new SearchQuery<Habit>(term, date));

            //ASSERT
            result.ShouldHaveValidationErrorFor(x => x.Date).WithErrorMessage("The date must be within the next 7 days.");
        }
    }
}
