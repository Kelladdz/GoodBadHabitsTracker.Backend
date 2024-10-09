using FluentValidation;
using FluentValidation.Results;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Abstractions.Behaviors
{
    internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICommandBase
    { 
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var errors = validators.Select(validator => validator.Validate(context))
                .Where(validationResult => !validationResult.IsValid)
                .SelectMany(validationResult => validationResult.Errors)
                .Select(validationResult => new ValidationError(
                    validationResult.PropertyName,
                    validationResult.ErrorMessage
                ))
                .ToList();

            if (errors.Count > 0)
            {
                throw new Exceptions.ValidationException(errors);
            }

            return await next();
        }
    }
}
