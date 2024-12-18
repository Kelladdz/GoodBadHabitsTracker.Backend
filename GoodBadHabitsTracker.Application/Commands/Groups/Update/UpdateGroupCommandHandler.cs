using FluentValidation;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using LanguageExt.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    internal sealed class UpdateGroupCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor,
        IValidator<Group> validator) : IRequestHandler<UpdateGroupCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

            var userId = user.Id;
            var groupId = command.Id;
            var document = command.Request;

            dbContext.BeginTransaction();

            try
            {
                var groupToUpdate = await dbContext.ReadGroupByIdAsync(groupId, userId);
                if (groupToUpdate == null)
                {
                    await dbContext.CommitAsync();
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Group Not Found"));
                }

                document.ApplyTo(groupToUpdate);
                var validationResult = await validator.ValidateAsync(groupToUpdate, cancellationToken);

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
