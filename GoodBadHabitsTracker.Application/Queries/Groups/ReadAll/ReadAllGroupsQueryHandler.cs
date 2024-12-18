using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using LanguageExt.Common;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Persistance;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadAll
{
    internal sealed class ReadAllGroupsQueryHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<ReadAllGroupsQuery, Result<IEnumerable<GroupResponse>>>
    {
        public async Task<Result<IEnumerable<GroupResponse>>> Handle(ReadAllGroupsQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<IEnumerable<GroupResponse>>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;

            dbContext.BeginTransaction();

            try
            {
                var allGroups = await dbContext.ReadAllGroupsAsync(userId);

                if (!allGroups.Any())
                {
                    await dbContext.CommitAsync();
                    return new Result<IEnumerable<GroupResponse>>([]);
                }

                var response = new List<GroupResponse>();
                foreach (var group in allGroups)
                {
                    response.Add(new GroupResponse(group));
                }

                await dbContext.CommitAsync();
                return new Result<IEnumerable<GroupResponse>>(response);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<IEnumerable<GroupResponse>>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}
