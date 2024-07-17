using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habit.GoodHabit.Create
{
    public class CreateGoodHabitCommandValidatorTests : ValidatorTestBase<CreateGoodHabitCommand>
    {
        private readonly CreateGoodHabitCommandValidator _validator;
        private readonly DataGenerator _generator;
        private readonly List<string> DaysOfWeek = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        public CreateGoodHabitCommandValidatorTests()
        {
            _validator = new CreateGoodHabitCommandValidator();
            _generator = new DataGenerator();
        }

        [Fact]
        public void ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => { };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
        }

        [Fact]
        public void NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.Name = "";

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required.");
        }

        [Fact]
        public void NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.Name = _generator.SeedTooLongHabitName();

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name must not exceed 100 characters.");
        }
        [Fact]
        public void IsGoodFalse_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.IsGood = false;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsGood).WithErrorMessage("For good habit IsGood should be true.");
        }
        [Fact]
        public void IsQuitNotNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.IsQuit = false;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsQuit).WithErrorMessage("IsQuit shouldn't be set if habit is good.");
        }
        [Fact]
        public void QuantityNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.Quantity = null;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For good habit, quantity shouldn't be null");
        }
        [Fact]
        public void InvalidFrequency_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.Frequency = null;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("Invalid frequency");
        }
        [Fact]
        public void InvalidRepeatMode_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.RepeatMode = null;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("Repeat mode must be Daily, Weekly or Monthly.");
        }
        [Fact]
        public void RepeatDaysOfWeekEmpty_RepeatModeDaily_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;
                x.Request.RepeatDaysOfWeek = [];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("RepeatDaysOfWeek shouldn't be empty if repeat mode is 'Daily'.");
        }
        [Fact]
        public void RepeatDaysOfWeekHasWrongDays_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;
                x.Request.RepeatDaysOfWeek = ["invalidday"];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("Invalid day of week: invalidday");
        }
        [Fact]
        public void RepeatDaysOfWeekNotEmpty_RepeatModeIsntDaily_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Monthly;
                x.Request.RepeatDaysOfWeek = ["Monday"];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("RepeatDaysOfWeek should be empty if repeat mode isn't 'Daily'.");
        }
        [Fact]
        public void RepeatIntervalZero_RepeatModeInterval_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Interval;
                x.Request.RepeatInterval = 0;
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("Repeat interval value shouldn't be 0 if repeat mode is interval");
        }
        [Fact]
        public void RepeatIntervalHasWrongValue_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Interval;
                x.Request.RepeatInterval = 9;
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("Repeat interval value must be greater than 1 and less than 8.");
        }
        [Fact]
        public void RepeatIntervalNotZero_RepeatModeIsntInterval_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;
                x.Request.RepeatInterval = 2;
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("Repeat interval value should be 0 if repeat mode isn't 'Interval'.");
        }
        [Fact]
        public void RepeatDaysOfMonthEmpty_RepeatModeMonthly_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Monthly;
                x.Request.RepeatDaysOfMonth = [];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("RepeatDaysOfMonth shouldn't be empty if repeat mode is 'Monthly'.");
        }
        [Fact]
        public void RepeatDaysOfMonthHasWrongDays_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Monthly;
                x.Request.RepeatDaysOfMonth = [33];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("Invalid day of month: 33");
        }
        [Fact]
        public void RepeatDaysOfMonthNotEmpty_RepeatModeIsntMonthly_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x =>
            {
                x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;
                x.Request.RepeatDaysOfMonth = [1, 2, 3, 4];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("RepeatDaysOfMonth should be empty if repeat mode isn't 'Monthly'.");
        }
        [Fact]
        public void StartDateInPast_ThrowsException()
        {
            //ARRANGE
            Action<CreateGoodHabitCommand> mutation = x => x.Request.StartDate = new DateOnly(2024,7,14);


            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date must be today or later.");
        }
        protected override CreateGoodHabitCommand CreateValidObject() 
            => new CreateGoodHabitCommand(_generator.SeedGoodHabitRequest());

        protected override IValidator<CreateGoodHabitCommand> CreateValidator()
            => new CreateGoodHabitCommandValidator();
    }
}
