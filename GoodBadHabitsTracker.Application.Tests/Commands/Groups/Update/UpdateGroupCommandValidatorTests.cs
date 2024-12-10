using FluentValidation.TestHelper;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Update
{
    public class UpdateGroupCommandValidatorTests
    {
        private readonly UpdateGroupCommandValidator _validator;
        public UpdateGroupCommandValidatorTests()
        {
            _validator = new();
        }
        
        [Fact]
        public void ShouldntThrowException_WhenRequestIsValid()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = DataGenerator.SeedGroupJsonPatchDocument();

            //ACT
            var result = _validator.TestValidate(new UpdateGroupCommand(id, request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].op");
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].path");
            result.ShouldNotHaveValidationErrorFor("Request.Operations[0].value");
        }
        [Fact]
        public void ShouldThrowException_WhenNameIsInvalid()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument();
            request.Operations.Add(new Operation
            {
                op = "replace",
                path = "/name",
                value = DataGenerator.SeedRandomString(2)
            });
            var command = new UpdateGroupCommand(id, request);
            //ACT
            var result = _validator.TestValidate(command);
            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("Invalid Name");
        }
        [Fact]
        public void ShouldThrowException_WhenTryToChangeUserId()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var request = new JsonPatchDocument();
            request.Operations.Add(new Operation
            {
                op = "replace",
                path = "/userId",
                value = Guid.NewGuid()
            });
            var command = new UpdateGroupCommand(id, request);
            //ACT
            var result = _validator.TestValidate(command);
            //ASSERT
            result.ShouldHaveValidationErrorFor("Request.Operations[0].value").WithErrorMessage("UserId is not allowed to be updated");
        }
    }
}
