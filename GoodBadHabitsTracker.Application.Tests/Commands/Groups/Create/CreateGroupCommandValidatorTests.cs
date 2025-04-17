using Bogus;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using FluentValidation.TestHelper;
using Bogus.DataSets;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Create
{
    public class CreateGroupCommandValidatorTests
    {
        private readonly CreateGroupCommandValidator _validator;

        public CreateGroupCommandValidatorTests()
        {
            _validator = new CreateGroupCommandValidator();
        }

        [Fact]
        public void ShouldntThrowException_WhenRequestIsValid()
        {
            //ARRANGE
            var name = new Faker<string>().CustomInstantiator(f => DataGenerator.SeedRandomString(10)).Generate();
            var request = new GroupRequest(name);

            //ACT
            var result = _validator.TestValidate(new CreateGroupCommand(request));

            //ASSERT
            result.ShouldNotHaveValidationErrorFor(model => model.Request.Name);
        }

        [Fact]
        public void NameIsNull_ThrowsException()
        {
            //ARRANGE
            var request = new GroupRequest(null);

            //ACT
            var result = _validator.TestValidate(new CreateGroupCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name cannot be null");
        }

        [Fact]
        public void NameIsEmpty_ThrowsException()
        {
            //ARRANGE
            var name = string.Empty;
            var request = new GroupRequest(name);


            //ACT
            var result = _validator.TestValidate(new CreateGroupCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name is required");
        }

        [Fact]
        public void NameIsTooShort_ThrowsException()
        {
            //ARRANGE
            var name = DataGenerator.SeedRandomString(2);
            var request = new GroupRequest(name);

            //ACT
            var result = _validator.TestValidate(new CreateGroupCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should be at least 3 characters");
        }

        [Fact]
        public void NameIsTooLong_ThrowsException()
        {
            //ARRANGE
            var name = DataGenerator.SeedRandomString(16);
            var request = new GroupRequest(name);

            //ACT
            var result = _validator.TestValidate(new CreateGroupCommand(request));

            //ASSERT
            result.ShouldHaveValidationErrorFor(model => model.Request.Name).WithErrorMessage("Name should not exceed 15 characters");
        }
    }
}