using FluentValidation;

namespace GoodBadHabitsTracker.Application.Queries.Generic.ReadById
{
       public sealed class ReadByIdQueryValidator<TEntity> : AbstractValidator<ReadByIdQuery<TEntity>>
            where TEntity : class
        {
            public ReadByIdQueryValidator()
            {
                RuleFor(x => x.Id)
                    .NotNull().WithMessage("Id is required.")
                    .Must(x => x.GetType() == typeof(Guid)).WithMessage("Id must be a valid GUID");
            }        
        }
}
