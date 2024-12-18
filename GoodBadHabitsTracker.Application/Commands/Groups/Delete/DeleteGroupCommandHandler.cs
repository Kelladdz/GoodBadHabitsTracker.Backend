using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Delete
{
    internal sealed class DeleteGroupCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<DeleteGroupCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

            var userId = user.Id;
            var groupId = command.Id;

            dbContext.BeginTransaction();

            try
            {
                var groupToDelete = await dbContext.ReadGroupByIdAsync(groupId, userId);
                if (groupToDelete == null)
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Group Not Found"));

                dbContext.DeleteGroup(groupToDelete);
                await dbContext.CommitAsync();

                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}
