using Bogus;
using GoodBadHabitsTracker.Application.Commands.Comments.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using FluentValidation.TestHelper;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Comments.Create
{
    public class CreateCommentCommandValidatorTests
    {
        private readonly CreateCommentCommandValidator _validator;

        public CreateCommentCommandValidatorTests()
        {
            _validator = new CreateCommentCommandValidator();
        }

        [Fact]
        public void ShouldntThrowException_WhenRequestIsValid()
        {
            //ARRANGE
            var request = DataGenerator.SeedCreateCommentRequest();

            //ACT
            var result = _validator.TestValidate(new CreateCommentCommand(Guid.NewGuid(), request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request);
        }

        [Fact]
        public void ShouldThrowException_WhenBodyIsEmpty()
        {
            //ARRANGE
            var request = new CreateCommentRequest
            {
                Body = "",
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            //ACT
            var result = _validator.TestValidate(new CreateCommentCommand(Guid.NewGuid(), request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Body).WithErrorMessage("Comment body is required");
        }

        [Fact]
        public void ShouldThrowException_WhenBodyTooLong()
        {
            //ARRANGE
            var request = new CreateCommentRequest
            {
                Body = DataGenerator.SeedRandomString(256),
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            //ACT
            var result = _validator.TestValidate(new CreateCommentCommand(Guid.NewGuid(), request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Body).WithErrorMessage("Your comment is too long");
        }
    }
}
