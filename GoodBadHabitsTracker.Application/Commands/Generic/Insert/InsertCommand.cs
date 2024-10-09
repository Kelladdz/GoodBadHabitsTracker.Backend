using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Insert
{
    public sealed record InsertCommand<TEntity, TRequest>(TRequest Request) : ICommand<GenericResponse<TEntity>>
         where TRequest : class
         where TEntity : class;
}
