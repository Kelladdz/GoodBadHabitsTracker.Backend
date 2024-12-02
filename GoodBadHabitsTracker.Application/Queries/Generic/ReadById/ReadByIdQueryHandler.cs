using MediatR;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.ReadById
{
    internal sealed class ReadByIdQueryHandler<TEntity> : IRequestHandler<ReadByIdQuery<TEntity>, GenericResponse<TEntity>>
            where TEntity : class
        {
            private readonly IGenericRepository<TEntity> _genericRepository;

            public ReadByIdQueryHandler(IGenericRepository<TEntity> genericRepository)
            {
                _genericRepository = genericRepository;
            }
            public async Task<GenericResponse<TEntity>> Handle(ReadByIdQuery<TEntity> query, CancellationToken cancellationToken)
            {
                var habitId = query.Id;

                var entity = await _genericRepository.ReadByIdAsync(habitId, cancellationToken);
                if (entity is null)
                    return null;

                var response = new GenericResponse<TEntity>(entity);

                return response;
            }
        }

}
