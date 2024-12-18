using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using LanguageExt.Common;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.Data.SqlClient;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Create
{
    internal sealed class CreateGroupCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<CreateGroupCommand, Result<GroupResponse>>
    {
        public async Task<Result<GroupResponse>> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<GroupResponse>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

            var userId = user.Id;
            var request = command.Request;
            var groupToInsert = new Group
            {
                Name = request.Name,
                UserId = userId
            };

            dbContext.BeginTransaction();

            try
            {
                await dbContext.InsertGroupAsync(groupToInsert);

                await dbContext.CommitAsync();
                return new Result<GroupResponse>(new GroupResponse(groupToInsert));
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<GroupResponse>(new AppException(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}
