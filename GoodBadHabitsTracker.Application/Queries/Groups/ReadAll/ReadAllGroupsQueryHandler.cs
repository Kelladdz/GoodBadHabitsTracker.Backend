﻿using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using LanguageExt.Common;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadAll
{
    internal sealed class ReadAllGroupsQueryHandler(IGroupsRepository groupsRepository, IUserAccessor userAccessor) : IRequestHandler<ReadAllGroupsQuery, Result<IEnumerable<GroupResponse>>>
    {
        public async Task<Result<IEnumerable<GroupResponse>>> Handle(ReadAllGroupsQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<IEnumerable<GroupResponse>>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;
            var groups = await groupsRepository.ReadAllAsync(userId, cancellationToken);
            if (groups is null)
                return new Result<IEnumerable<GroupResponse>>([]);

            var response = new List<GroupResponse>();
            foreach (var group in groups)
            {
                response.Add(new GroupResponse(group));
            }

            return new Result<IEnumerable<GroupResponse>>(response);
        }
    }
}