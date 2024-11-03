using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Queries.Generic.GetAll
{
    public sealed record GetAllQuery<TEntity>() : ICommand<IEnumerable<GenericResponse<TEntity>>> where TEntity : class;
}
