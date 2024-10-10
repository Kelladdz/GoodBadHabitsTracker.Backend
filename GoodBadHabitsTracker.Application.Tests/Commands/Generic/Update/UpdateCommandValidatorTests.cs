using FluentValidation.TestHelper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Generic.Update
{
    public class UpdateCommandValidatorTests
    {
        private readonly UpdateCommandValidator<Habit> _habitsValidator;
        private readonly UpdateCommandValidator<Group> _groupsValidator;
        private readonly DataGenerator _dataGenerator;

        public UpdateCommandValidatorTests()
        {
            _habitsValidator = new();
            _groupsValidator = new();
            _dataGenerator = new();
        }

        [Fact]
        public void Habit_ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/name",
                value = "New name"
            });

            //ACT
            var result = _habitsValidator.TestValidate(new UpdateCommand<Habit>(id, request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Operations[0].op);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Operations[0].path);
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Operations[0].value);
        }

        [Fact]
        public void Habit_InvalidDocumentOp_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "invalidOp",
                path = "/name",
                value = "New name"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].op").WithErrorMessage("Invalid operation type");
        }

        [Fact]
        public void Habit_InvalidDocumentPath_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "invalidPath",
                value = "something"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid path");
        }

        [Fact]
        public void Habit_InvalidNameValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/name",
                value = _dataGenerator.SeedRandomString(2)
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid Name");
        }

        [Fact]
        public void Habit_TryToChangeHabitType_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/habitType",
                value = 2
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("HabitType is not allowed to be updated");
        }

        [Fact]
        public void Habit_InvalidStartDateValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/startDate",
                value = _dataGenerator.SeedRandomString(16)
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid StartDate");
        }

        [Fact]
        public void Habit_TryToChangeIsTimeBasedFlag_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/isTimeBased",
                value = "true"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("IsTimeBased is not allowed to be updated");
        }

        [Fact]
        public void Habit_InvalidQuantityValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/quantity",
                value = _dataGenerator.SeedRandomString(16)
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid Quantity");
        }

        [Fact]
        public void Habit_InvalidFrequencyValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/frequency",
                value = 6
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid Frequency");
        }

        [Fact]
        public void Habit_InvalidRepeatModeValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/repeatMode",
                value = 6
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid RepeatMode");
        }

        [Fact]
        public void Habit_InvalidRepeatDaysOfMonthValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/repeatDaysOfMonth/-",
                value = 50
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid day of month");
        }

        [Fact]
        public void Habit_InvalidRepeatDaysOfWeekValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/repeatDaysOfWeek/-",
                value = 10
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid day of week");
        }

        [Fact]
        public void Habit_InvalidRepeatIntervalValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "replace",
                path = "/repeatInterval",
                value = 10
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid interval value");
        }

        [Fact]
        public void Habit_InvalidReminderTimeValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/reminderTimes",
                value = "25:00"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid time");
        }

        [Fact]
        public void Habit_CommentIsTooLong_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();
            var comment = new Comment
            {
                Body = _dataGenerator.SeedRandomString(256),
                HabitId = id
            };

            var commentJson = JsonConvert.SerializeObject(comment);

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/comments",
                value = commentJson
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Comment is too long");
        }

        [Fact]
        public void Habit_CommentIsEmptyString_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();
            var comment = new Comment
            {
                Body = "",
                HabitId = id
            };

            var commentJson = JsonConvert.SerializeObject(comment);

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/comments",
                value = commentJson
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Comment cannot be empty");
        }

        [Fact]
        public void Habit_InvalidDayResultStatus_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();
            var dayResult = new DayResult
            {
                Status = Core.Enums.Statuses.Completed,
                Progress = -1800,
                HabitId = id
            };

            var dayResultJson = JsonConvert.SerializeObject(dayResult);

            request.Operations.Add(new Operation<Habit>
            {
                op = "add",
                path = "/dayResults",
                value = dayResultJson
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Progress should be greater than or equal to 0");
        }

        [Fact]
        public void Habit_Remove_InvalidNamePath_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "remove",
                path = "/name/2"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Name is not allowed to remove");
        }

        [Fact]
        public void Habit_Remove_NotAllowedProperties_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Habit>();

            request.Operations.Add(new Operation<Habit>
            {
                op = "remove",
                path = "/name"
            });

            var command = new UpdateCommand<Habit>(id, request);

            //ACT
            var result = _habitsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Name is not allowed to remove");
        }

        [Fact]
        public void Group_ValidRequest_DoesntThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Group>();

            request.Operations.Add(new Operation<Group>
            {
                op = "replace",
                path = "/name",
                value = "New name"
            });

            //ACT
            var result = _groupsValidator.TestValidate(new UpdateCommand<Group>(id, request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].op");
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].path");
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].value");
        }

        [Fact]
        public void Group_InvalidNameValue_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Group>();

            request.Operations.Add(new Operation<Group>
            {
                op = "replace",
                path = "/name",
                value = _dataGenerator.SeedRandomString(2)
            });

            var command = new UpdateCommand<Group>(id, request);

            //ACT
            var result = _groupsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid Name");
        }

        [Fact]
        public void Group_TryToChangeUserId_ThrowsException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument<Group>();

            request.Operations.Add(new Operation<Group>
            {
                op = "replace",
                path = "/userId",
                value = Guid.NewGuid()
            });

            var command = new UpdateCommand<Group>(id, request);

            //ACT
            var result = _groupsValidator.TestValidate(command);

            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("UserId is not allowed to be updated");
        }
    }
}
