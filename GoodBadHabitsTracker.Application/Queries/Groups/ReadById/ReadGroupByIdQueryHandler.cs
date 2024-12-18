using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Interfaces;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Persistance;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadById
{
    internal sealed class ReadGroupByIdQueryHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<ReadGroupByIdQuery, Result<GroupResponse>>
    {
        public async Task<Result<GroupResponse>> Handle(ReadGroupByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<GroupResponse>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;
            var groupId = query.Id;

            dbContext.BeginTransaction();

            try
            {
                var group = await dbContext.ReadGroupByIdAsync(userId, groupId);
                if (group is null)
                {
                    await dbContext.CommitAsync();
                    return new Result<GroupResponse>(new AppException(HttpStatusCode.NotFound, "Group Not Found"));
                }

                await dbContext.CommitAsync();
                return new Result<GroupResponse>(new GroupResponse(group));
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<GroupResponse>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}
