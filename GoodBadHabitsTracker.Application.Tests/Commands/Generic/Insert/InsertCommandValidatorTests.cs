using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.TestMisc;


namespace GoodBadHabitsTracker.Application.Tests.Commands.Generic.Insert
{
    public class InsertCommandValidatorTests 
    {
        private readonly InsertCommandValidator<Core.Models.Habit, HabitRequest> _habitsValidator;
        private readonly InsertCommandValidator<Core.Models.Group, GroupRequest> _groupsValidator;
        private readonly DataGenerator _dataGenerator;

        public InsertCommandValidatorTests()
        {
            _habitsValidator = new InsertCommandValidator<Core.Models.Habit, HabitRequest>();
            _groupsValidator = new InsertCommandValidator<Core.Models.Group, GroupRequest>();
            _dataGenerator = new DataGenerator();
        }

       [Fact]
        public void Habit_ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

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
        public void Habit_NameIsNull_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.Name = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name cannot be null");
        }

        [Fact]
        public void Habit_NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.Name = "";

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required");
        }

        [Fact]
        public void Habit_NameIsTooShort_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.Name = _dataGenerator.SeedRandomString(2);

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should be at least 3 characters");
        }

        [Fact]
        public void Habit_NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.Name = _dataGenerator.SeedRandomString(51);

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should not exceed 50 characters");
        }

        [Fact]
        public void Habit_IconPathIsNull_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.IconId = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IconId).WithErrorMessage("Icon path cannot be null");
        }

        [Fact]
        public void Habit_HabitTypeIsNull_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.HabitType = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.HabitType).WithErrorMessage("Habit type cannot be null");
        }

        [Fact]
        public void Habit_HabitTypeInNotInEnum_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.HabitType = HabitTypes.Quit + 1;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.HabitType).WithErrorMessage("Invalid habit type");
        }

        [Fact]
        public void Habit_StartDateIsNull_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.StartDate = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date cannot be null");
        }

        [Fact]
        public void Habit_StartDateInPast_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();

            request.StartDate = new DateOnly(2023, 7, 14);

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.StartDate).WithErrorMessage("Start date should be today or later");
        }

        [Fact]
        public void Habit_IsTimeBasedFlagIsNotNullAndHabitTypeIsQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedQuitHabitRequest();

            request.HabitType = HabitTypes.Quit;
            request.IsTimeBased = true;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsTimeBased).WithErrorMessage("For breaking a habit, IsTimeBased should be null");
        }

        [Fact]
        public void Habit_IsTimeBasedFlagIsNullAndHabitTypeIsNotQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.IsTimeBased = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.IsTimeBased).WithErrorMessage("For good and limiting habit, IsTimeBased flag cannot be null");
        }

        [Fact]
        public void Habit_QuantityIsNotNullAndHabitTypeIsQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedQuitHabitRequest();

            request.Quantity = 100;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For breaking a habit, Quantity should be null");
        }

        [Fact]
        public void Habit_QuantityIsNullAndHabitTypeIsNotQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.Quantity = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Quantity).WithErrorMessage("For good and limiting habit Quantity cannot be null");
        }

        [Fact]
        public void Habit_FrequencyIsNotNullAndHabitTypeIsQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedQuitHabitRequest();

            request.Frequency = Frequencies.PerDay;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("For breaking a habit, Frequency should be null");
        }

        [Fact]
        public void Habit_FrequencyIsNullAndHabitTypeIsNotQuit_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.Frequency = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("For good and limiting habit, Frequency cannot be null");
        }

        [Fact]
        public void Habit_FrequencyInNotInEnum_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.Frequency = Frequencies.PerMonth + 1;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Frequency).WithErrorMessage("Invalid Frequency");
        }

        [Fact]
        public void Habit_RepeatModeIsNotNullAndHabitTypeIsNotGood_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedQuitHabitRequest();

            request.RepeatMode = RepeatModes.Daily;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For breaking or limiting a habit, RepeatMode should be null");
        }

        [Fact]
        public void Habit_RepeatModeIsNullAndHabitTypeIsGood_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.RepeatMode = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("For good habit, RepeatMode cannot be null");
        }

        [Fact]
        public void Habit_RepeatModeInNotInEnum_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedGoodHabitRequest();

            request.RepeatMode = RepeatModes.Interval + 1;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatMode).WithErrorMessage("Invalid RepeatMode");
        }

        [Fact]
        public void Habit_RepeatDaysOfWeekHasInvalidDayValue_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfWeek = [DayOfWeek.Saturday + 1];

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("Invalid day of week");
        }
        [Fact]
        public void Habit_RepeatDaysOfWeekIsNotNullAndRepeatModeIsNotDaily_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfWeek = [DayOfWeek.Monday];

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("If habit isn't in daily repeat mode, then RepeatDaysOfWeek should be null");
        }
        [Fact]
        public void Habit_RepeatDaysOfWeekIsNullAndRepeatModeIsDaily_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfWeek = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfWeek).WithErrorMessage("If habit is in daily repeat mode, then RepeatDaysOfWeek cannot be null");
        }

        [Fact]
        public void Habit_RepeatDaysOfMonthHasInvalidDayValue_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfMonth = [44];

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("Invalid day of month: 44");
        }
        [Fact]
        public void Habit_RepeatDaysOfMonthIsNotNullAndRepeatModeIsNotMonthly_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatDaysOfMonth = [13];

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("If habit isn't in monthly repeat mode, then RepeatDaysOfMonth should be null");
        }
        [Fact]
        public void Habit_RepeatDaysOfMonthIsNullAndRepeatModeIsMonthly_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInMonthlyRepeatModeRequest();

            request.RepeatDaysOfMonth = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatDaysOfMonth).WithErrorMessage("If habit is in monthly repeat mode, then RepeatDaysOfMonth cannot be null");
        }

        [Fact]
        public void Habit_RepeatIntervalIsInvalid_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInIntervalRepeatModeRequest();

            request.RepeatInterval = 10;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("Interval should be greater than 1 and less than 8");
        }
        [Fact]
        public void Habit_RepeatIntervalIsValidIntervalValueAndRepeatModeIsNotInterval_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInDailyRepeatModeRequest();

            request.RepeatInterval = 2;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("If habit isn't in interval repeat mode, then RepeatInterval should be null");
        }
        [Fact]
        public void Habit_RepeatIntervalIsNullAndRepeatModeIsInterval_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitInIntervalRepeatModeRequest();

            request.RepeatInterval = null;

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.RepeatInterval).WithErrorMessage("If habit is in interval repeat mode, then RepeatInterval cannot be null");
        }

        [Fact]
        public void Habit_ReminderTimesIsNotNullAndHabitTypeIsNotGood_ThrowsException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLimitHabitRequest();

            request.ReminderTimes = [new TimeOnly(10, 30)];

            //ACT
            var result = _habitsValidator.TestValidate(new InsertCommand<Core.Models.Habit, HabitRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.ReminderTimes).WithErrorMessage("For breaking or limiting habit, ReminderTimes should be null");
        }

        [Fact]
        public void Group_ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest
            {
                Name = _dataGenerator.SeedRandomString(10)
            };

            //ACT
            var result = _groupsValidator.TestValidate(new InsertCommand<Core.Models.Group, GroupRequest>(request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
        }

        [Fact]
        public void Group_NameIsNull_ThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest
            {
                Name = null
            };

            //ACT
            var result = _groupsValidator.TestValidate(new InsertCommand<Core.Models.Group, GroupRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name cannot be null");
        }

        [Fact]
        public void Group_NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest
            {
                Name = ""
            };

            //ACT
            var result = _groupsValidator.TestValidate(new InsertCommand<Core.Models.Group, GroupRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required");
        }

        [Fact]
        public void Group_NameIsTooShort_ThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest
            {
                Name = _dataGenerator.SeedRandomString(2)
            };

            //ACT
            var result = _groupsValidator.TestValidate(new InsertCommand<Core.Models.Group, GroupRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should be at least 3 characters");
        }

        [Fact]
        public void Group_NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest
            {
                Name = _dataGenerator.SeedRandomString(51)
            };

            //ACT
            var result = _groupsValidator.TestValidate(new InsertCommand<Core.Models.Group, GroupRequest>(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should not exceed 15 characters");
        }
    }
}
