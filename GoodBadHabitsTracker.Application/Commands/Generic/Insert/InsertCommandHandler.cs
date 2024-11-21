using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Insert
{
    internal sealed class InsertCommandHandler<TEntity, TRequest> : IRequestHandler<InsertCommand<TEntity, TRequest>, GenericResponse<TEntity>>
        where TEntity : class
        where TRequest : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InsertCommandHandler(IGenericRepository<TEntity> genericRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _genericRepository = genericRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GenericResponse<TEntity>> Handle(InsertCommand<TEntity, TRequest> request, CancellationToken cancellationToken)
        {
            var entityToInsert = _mapper.Map<TEntity>(request.Request);
            var accessToken = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = Guid.Parse(new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);

            var newEntity = await _genericRepository.InsertAsync(entityToInsert, userId, cancellationToken);
            var response = newEntity is not null ? new GenericResponse<TEntity>(newEntity) : null;

            return response;
        }
    }
}
