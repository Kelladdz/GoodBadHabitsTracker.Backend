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
            var userId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a");

            var newEntity = await _genericRepository.InsertAsync(entityToInsert, userId, cancellationToken);
            var response = new GenericResponse<TEntity>(newEntity);

            return response;
        }
    }

}
