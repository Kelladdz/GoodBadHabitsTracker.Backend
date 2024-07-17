using FluentValidation;
using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create;
using GoodBadHabitsTracker.TestMisc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habit.LimitHabit.Create
{
    public class CreateLimitHabitCommandValidatorTests : ValidatorTestBase<CreateLimitHabitCommand>
    {
        private readonly CreateLimitHabitCommandValidator _validator;
        private readonly DataGenerator _generator;
        public CreateLimitHabitCommandValidatorTests()
        {
            _validator = new CreateLimitHabitCommandValidator();
            _generator = new DataGenerator();
        }

        [Fact]
        public void ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => { };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
        }

        [Fact]
        public void NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.Name = "";

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required.");
        }

        [Fact]
        public void NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.Name = _generator.SeedTooLongHabitName();

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name must not exceed 100 characters.");
        }
        [Fact]
        public void IsGoodTrue_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.IsGood = true;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsGood).WithErrorMessage("For limiting habit IsGood should be false.");
        }
        [Fact]
        public void IsQuitNotFalse_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.IsQuit = true;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsQuit).WithErrorMessage("IsQuit should be false set for limiting habit.");
        }
        [Fact]
        public void QuantityNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.Quantity = null;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For limiting habit, quantity shouldn't be null.");
        }
        [Fact]
        public void InvalidFrequency_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.Frequency = null;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("Invalid frequency");
        }
        [Fact]
        public void RepeatModeNotNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For limiting habit, repeat mode should be null.");
        }
        [Fact]
        public void RepeatDaysOfWeekNotEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x =>
            {
                x.Request.RepeatDaysOfWeek = ["Monday"];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("For limiting habit, list of repeat days should be empty.");
        }
        
        [Fact]
        public void RepeatIntervalNotZero_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x =>
            {
                x.Request.RepeatInterval = 2;
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("For limiting habit, repeat interval value should be 0.");
        }
         
        [Fact]
        public void RepeatDaysOfMonthNotEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x =>
            {
                x.Request.RepeatDaysOfMonth = [10];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("For limiting habit, list of repeat days should be empty.");
        }

        [Fact]
        public void StartDateInPast_ThrowsException()
        {
            //ARRANGE
            Action<CreateLimitHabitCommand> mutation = x => x.Request.StartDate = new DateOnly(2024, 7, 14);

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date must be today or later.");
        }
        protected override CreateLimitHabitCommand CreateValidObject()
            => new CreateLimitHabitCommand(_generator.SeedLimitHabitRequest());

        protected override IValidator<CreateLimitHabitCommand> CreateValidator()
            => new CreateLimitHabitCommandValidator();
    }
}
