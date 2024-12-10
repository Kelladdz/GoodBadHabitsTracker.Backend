using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.TestMisc;
using FluentValidation.TestHelper;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.Create
{
    public class CreateHabitCommandValidatorTests
    {
        private readonly CreateHabitCommandValidator _validator;

        public CreateHabitCommandValidatorTests()
        {
            _validator = new CreateHabitCommandValidator();
        }

        [Fact]
        public void ShouldntThrowException_WhenRequestIsValid()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.IconId);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.HabitType);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.StartDate);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.IsTimeBased);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Quantity);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Frequency);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.RepeatMode);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.RepeatInterval);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.ReminderTimes);
        }

        [Fact]
        public void ShouldThrowException_WhenNameIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.Name = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenNameIsEmpty()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.Name = "";

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required");
        }

        [Fact]
        public void ShouldThrowException_WhenNameIsTooShort()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.Name = DataGenerator.SeedRandomString(2);

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should be at least 3 characters");
        }

        [Fact]
        public void ShouldThrowException_WhenNameIsTooLong()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.Name = DataGenerator.SeedRandomString(51);

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should not exceed 50 characters");
        }

        [Fact]
        public void ShouldThrowException_WhenIconIdIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.IconId = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IconId).WithErrorMessage("Icon path cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenHabitTypeIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.HabitType = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.HabitType).WithErrorMessage("Habit type cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenHabitTypeInNotInEnum()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.HabitType = HabitTypes.Quit + 1;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.HabitType).WithErrorMessage("Invalid habit type");
        }

        [Fact]
        public void ShouldThrowException_WhenStartDateIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.StartDate = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenStartDateInPast()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitRequest();

            request.StartDate = new DateOnly(2023, 7, 14);

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date should be today or later");
        }

        [Fact]
        public void ShouldThrowException_WhenIsTimeBasedFlagIsNotNullAndHabitTypeIsQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedQuitHabitRequest();

            request.HabitType = HabitTypes.Quit;
            request.IsTimeBased = true;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsTimeBased).WithErrorMessage("For breaking a habit, IsTimeBased should be null");
        }

        [Fact]
        public void ShouldThrowException_WhenIsTimeBasedFlagIsNullAndHabitTypeIsNotQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.IsTimeBased = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsTimeBased).WithErrorMessage("For good and limiting habit, IsTimeBased flag cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenQuantityIsNotNullAndHabitTypeIsQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedQuitHabitRequest();

            request.Quantity = 100;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For breaking a habit, Quantity should be null");
        }

        [Fact]
        public void ShouldThrowException_WhenQuantityIsNullAndHabitTypeIsNotQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.Quantity = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For good and limiting habit Quantity cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenFrequencyIsNotNullAndHabitTypeIsQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedQuitHabitRequest();

            request.Frequency = Frequencies.PerDay;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("For breaking a habit, Frequency should be null");
        }

        [Fact]
        public void ShouldThrowException_WhenFrequencyIsNullAndHabitTypeIsNotQuit()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.Frequency = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("For good and limiting habit, Frequency cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenFrequencyInNotInEnum()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.Frequency = Frequencies.PerMonth + 1;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("Invalid Frequency");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatModeIsNotNullAndHabitTypeIsNotGood()
        {
            //ARRANGE
            var request = DataGenerator.SeedQuitHabitRequest();

            request.RepeatMode = RepeatModes.Daily;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For breaking or limiting a habit, RepeatMode should be null");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatModeIsNullAndHabitTypeIsGood()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.RepeatMode = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For good habit, RepeatMode cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatModeInNotInEnum()
        {
            //ARRANGE
            var request = DataGenerator.SeedGoodHabitRequest();

            request.RepeatMode = RepeatModes.Interval + 1;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("Invalid RepeatMode");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfWeekHasInvalidDayValue()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfWeek = [DayOfWeek.Saturday + 1];

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("Invalid day of week");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfWeekIsNotNullAndRepeatModeIsNotDaily()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfWeek = [DayOfWeek.Monday];

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("If habit isn't in daily repeat mode, then RepeatDaysOfWeek should be null");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfWeekIsNullAndRepeatModeIsDaily()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfWeek = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("If habit is in daily repeat mode, then RepeatDaysOfWeek cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfMonthHasInvalidDayValue()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfMonth = [44];

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("Invalid day of month: 44");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfMonthIsNotNullAndRepeatModeIsNotMonthly()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfMonth = [13];

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("If habit isn't in monthly repeat mode, then RepeatDaysOfMonth should be null");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatDaysOfMonthIsNullAndRepeatModeIsMonthly()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfMonth = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("If habit is in monthly repeat mode, then RepeatDaysOfMonth cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenRepeatIntervalIsInvalid()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInIntervalRepeatModeRequest();

            request.RepeatInterval = 10;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("Interval should be greater than 1 and less than 8");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatIntervalIsValidIntervalValueAndRepeatModeIsNotInterval()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatInterval = 2;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("If habit isn't in interval repeat mode, then RepeatInterval should be null");
        }
        [Fact]
        public void ShouldThrowException_WhenRepeatIntervalIsNullAndRepeatModeIsInterval()
        {
            //ARRANGE
            var request = DataGenerator.SeedHabitInIntervalRepeatModeRequest();

            request.RepeatInterval = null;

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("If habit is in interval repeat mode, then RepeatInterval cannot be null");
        }

        [Fact]
        public void ShouldThrowException_WhenReminderTimesIsNotNullAndHabitTypeIsNotGood()
        {
            //ARRANGE
            var request = DataGenerator.SeedLimitHabitRequest();

            request.ReminderTimes = [new TimeOnly(10, 30)];

            //ACT
            var result = _validator.TestValidate(new CreateHabitCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.ReminderTimes).WithErrorMessage("For breaking or limiting habit, ReminderTimes should be null");
        }
    }
}
