using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.GetAll
{
    internal sealed class GetAllQueryHandler<TEntity> : IRequestHandler<GetAllQuery<TEntity>, IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _genericRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenHandler _tokenHandler;
        private readonly UserManager<User> _userManager;

        public GetAllQueryHandler(
            IGenericRepository<TEntity> genericRepository, 
            IHttpContextAccessor httpContextAccessor,
            ITokenHandler tokenHandler,
            UserManager<User> userManager)
        {
            _genericRepository = genericRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenHandler = tokenHandler;
            _userManager = userManager;
        }
        public async Task<IEnumerable<GenericResponse<TEntity>>> Handle(GetAllQuery<TEntity> request, CancellationToken cancellationToken)
        {
            var accessToken = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var sub = _tokenHandler.GetClaimsFromToken(accessToken).FirstOrDefault(claim => claim.Type == "sub")?.Value;
            User user;
            if (Guid.TryParse(sub, out Guid _))
            {
                user = await _userManager.FindByIdAsync(sub)
                    ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found.");
            }
            else
            {
                var provider = sub.Contains("google") ? "Google" : "Facebook";
                user = await _userManager.FindByLoginAsync(provider, sub)
                    ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found.");
            }     
            var entities = await _genericRepository.ReadAllAsync(user.Id, cancellationToken);
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
