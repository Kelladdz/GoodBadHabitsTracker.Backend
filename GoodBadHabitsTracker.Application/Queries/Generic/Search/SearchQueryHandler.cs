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
            var userId = Guid.Parse("5162ee1a-b50b-4972-92d8-08dce8c110ea");

            var entities = await _genericRepository.SearchAsync(term, date, userId, cancellationToken);
            if (entities is null)
                return null;

            var response = new List<GenericResponse<TEntity>>();
            foreach (var entity in entities)
            {
                response.Add(new GenericResponse<TEntity>(entity));
            }

            return response;
        }
    }

}
