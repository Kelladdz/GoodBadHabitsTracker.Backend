using MediatR;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    internal sealed class SearchQueryHandler<TEntity> : IRequestHandler<SearchQuery<TEntity>, IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SearchQueryHandler(IGenericRepository<TEntity> genericRepository, IHttpContextAccessor httpContextAccessor)
        {
            _genericRepository = genericRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<GenericResponse<TEntity>>> Handle(SearchQuery<TEntity> query, CancellationToken cancellationToken)
        {
            var term = query.Term;
            var date = query.Date;
            var accessToken = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);

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
