using FluentValidation;
using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create;
using GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Create;
using GoodBadHabitsTracker.TestMisc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habit.QuitHabit.Create
{
    public class CreateQuitHabitCommandValidatorTests : ValidatorTestBase<CreateQuitHabitCommand>
    {
        private readonly CreateQuitHabitCommandValidator _validator;
        private readonly DataGenerator _generator;
        public CreateQuitHabitCommandValidatorTests()
        {
            _validator = new CreateQuitHabitCommandValidator();
            _generator = new DataGenerator();
        }

        [Fact]
        public void ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => { };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
        }

        [Fact]
        public void NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.Name = "";

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required.");
        }

        [Fact]
        public void NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.Name = _generator.SeedTooLongHabitName();

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name must not exceed 100 characters.");
        }
        [Fact]
        public void IsGoodTrue_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.IsGood = true;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsGood).WithErrorMessage("For breaking habit IsGood should be false.");
        }
        [Fact]
        public void IsQuitNotTrue_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.IsQuit = false;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsQuit).WithErrorMessage("For breaking habit IsQuit should be true.");
        }
        [Fact]
        public void QuantityNotNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.Quantity = 100;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For breaking habit, quantity should be null.");
        }
        [Fact]
        public void FrequencyNotNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.Frequency = Core.Enums.Frequencies.PerDay;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("For breaking habit, frequency should be null.");
        }
        [Fact]
        public void RepeatModeNotNull_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.RepeatMode = Core.Enums.RepeatModes.Daily;

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For breaking habit, repeat mode should be null.");
        }
        [Fact]
        public void RepeatDaysOfWeekNotEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x =>
            {
                x.Request.RepeatDaysOfWeek = ["Monday"];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("For breaking habit, list of repeat days should be empty.");
        }

        [Fact]
        public void RepeatIntervalNotZero_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x =>
            {
                x.Request.RepeatInterval = 2;
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("For breaking habit, repeat interval value should be 0.");
        }

        [Fact]
        public void RepeatDaysOfMonthNotEmpty_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x =>
            {
                x.Request.RepeatDaysOfMonth = [10];
            };

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("For breaking habit, list of repeat days should be empty.");
        }

        [Fact]
        public void StartDateInPast_ThrowsException()
        {
            //ARRANGE
            Action<CreateQuitHabitCommand> mutation = x => x.Request.StartDate = new DateOnly(2024, 7, 14);

            //ACT
            var result = Validate(mutation);

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date must be today or later.");
        }
        protected override CreateQuitHabitCommand CreateValidObject()
            => new CreateQuitHabitCommand(_generator.SeedQuitHabitRequest());

        protected override IValidator<CreateQuitHabitCommand> CreateValidator()
            => new CreateQuitHabitCommandValidator();
    }
}
