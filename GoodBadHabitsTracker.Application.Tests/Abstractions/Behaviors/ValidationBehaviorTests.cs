using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GoodBadHabitsTracker.Application.Abstractions.Behaviors;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using MediatR;
using Moq;
using ValidationException = GoodBadHabitsTracker.Application.Exceptions.ValidationException;


namespace GoodBadHabitsTracker.Application.Tests.Abstractions.Behaviors
{
    public class ValidationBehaviorTests : ValidatorTestBase<CreateHabitCommand>
    {
        private readonly Mock<IValidator<CreateHabitCommand>> _validatorMock;
        private readonly Mock<RequestHandlerDelegate<HabitResponse>> _nextMock;
        private readonly ValidationBehavior<CreateHabitCommand, HabitResponse> _behavior;

        public ValidationBehaviorTests()
        {
            _validatorMock = new Mock<IValidator<CreateHabitCommand>>();
            _nextMock = new Mock<RequestHandlerDelegate<HabitResponse>>();
            _behavior = new ValidationBehavior<CreateHabitCommand, HabitResponse>(new[] { _validatorMock.Object });
        }

        [Fact]
        public async Task Handle_ShouldCallNext_WhenValidationPasses()
        {
            // Arrange
            var request = CreateValidObject();
            request.Request.Name = null;
            _validatorMock.Setup(v => v.Validate(It.IsAny<ValidationContext<CreateHabitCommand>>()))
                .Returns(new ValidationResult());

            // Act
            await _behavior.Handle(request, _nextMock.Object, default);

            // Assert
            _nextMock.Verify(n => n(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var request = CreateValidObject();
            var errors = new List<ValidationError>
        {
            new ValidationError("PropertyName", "ErrorMessage")
        };
            _validatorMock.Setup(v => v.Validate(It.IsAny<ValidationContext<CreateHabitCommand>>()))
                .Throws(new ValidationException(errors));

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _behavior.Handle(request, _nextMock.Object, default));
        }
        protected override CreateHabitCommand CreateValidObject()
            => new(DataGenerator.SeedHabitRequest());
        protected override IValidator<CreateHabitCommand> CreateValidator()
            => new CreateHabitCommandValidator();
    }

    public class TestRequest : IRequestBase { }
    public class HabitResponse { }
}
