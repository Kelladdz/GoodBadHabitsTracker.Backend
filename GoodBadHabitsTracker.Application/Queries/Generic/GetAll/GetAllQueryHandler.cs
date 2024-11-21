using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GoodBadHabitsTracker.Application.Queries.Generic.GetAll
{
    internal sealed class GetAllQueryHandler<TEntity> : IRequestHandler<GetAllQuery<TEntity>, IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetAllQueryHandler(IGenericRepository<TEntity> genericRepository, IHttpContextAccessor httpContextAccessor)
        {
            _genericRepository = genericRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<GenericResponse<TEntity>>> Handle(GetAllQuery<TEntity> request, CancellationToken cancellationToken)
        {
            var accessToken = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
            var entities = await _genericRepository.ReadAllAsync(userId, cancellationToken);
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
