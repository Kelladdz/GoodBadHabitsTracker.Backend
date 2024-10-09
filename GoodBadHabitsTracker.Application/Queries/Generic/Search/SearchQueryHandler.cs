using MediatR;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    internal sealed class SearchQueryHandler<TEntity> : IRequestHandler<SearchQuery<TEntity>, IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        public SearchQueryHandler(IGenericRepository<TEntity> genericRepository)
        {
            _genericRepository = genericRepository;
        }
        public async Task<IEnumerable<GenericResponse<TEntity>>> Handle(SearchQuery<TEntity> query, CancellationToken cancellationToken)
        {
            var term = query.Term;
            var date = query.Date;
            var userId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a");

            var entities = await _genericRepository.SearchAsync(term, date, userId, cancellationToken);

            var response = new List<GenericResponse<TEntity>>();
            foreach (var entity in entities)
            {
                response.Add(new GenericResponse<TEntity>(entity));
            }

            return response;
        }
    }

}
