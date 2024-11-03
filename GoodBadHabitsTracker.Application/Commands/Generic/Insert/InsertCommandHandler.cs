using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Insert
{
    internal sealed class InsertCommandHandler<TEntity, TRequest> : IRequestHandler<InsertCommand<TEntity, TRequest>, GenericResponse<TEntity>>
        where TEntity : class
        where TRequest : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IMapper _mapper;

        public InsertCommandHandler(IGenericRepository<TEntity> genericRepository, IMapper mapper)
        {
            _genericRepository = genericRepository;
            _mapper = mapper;
        }

        public async Task<GenericResponse<TEntity>> Handle(InsertCommand<TEntity, TRequest> request, CancellationToken cancellationToken)
        {
            var entityToInsert = _mapper.Map<TEntity>(request.Request);
            var userId = Guid.Parse("5162ee1a-b50b-4972-92d8-08dce8c110ea");

            var newEntity = await _genericRepository.InsertAsync(entityToInsert, userId, cancellationToken);
            var response = newEntity is not null ? new GenericResponse<TEntity>(newEntity) : null;

            return response;
        }
    }
}
