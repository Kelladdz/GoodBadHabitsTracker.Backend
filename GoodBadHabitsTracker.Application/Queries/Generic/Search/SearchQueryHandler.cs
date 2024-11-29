using MediatR;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GoodBadHabitsTracker.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    internal sealed class SearchQueryHandler<TEntity> : IRequestHandler<SearchQuery<TEntity>, IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIdTokenHandler _idTokenHandler;
        private readonly UserManager<User> _userManager;
        public SearchQueryHandler(IGenericRepository<TEntity> genericRepository, 
            IHttpContextAccessor httpContextAccessor,
            IIdTokenHandler idTokenHandler,
            UserManager<User> userManager)
        {
            _genericRepository = genericRepository;
            _httpContextAccessor = httpContextAccessor;
            _idTokenHandler = idTokenHandler;
            _userManager = userManager;
        }
        public async Task<IEnumerable<GenericResponse<TEntity>>> Handle(SearchQuery<TEntity> query, CancellationToken cancellationToken)
        {
            var term = query.Term;
            var date = query.Date;
            var accessToken = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userEmail = _idTokenHandler.GetClaimsPrincipalFromIdToken(accessToken).FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value;
            var user = await _userManager.FindByEmailAsync(userEmail)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found.");

            var entities = await _genericRepository.SearchAsync(term, date, user.Id, cancellationToken);
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
