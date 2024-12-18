using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.Application.DTOs.Response;
using Microsoft.Extensions.Logging;
using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Application.Commands.Comments.Create
{
    internal sealed class CreateCommentCommandHandler(IHabitsDbContext dbContext, IUserAccessor userAccessor, ILogger logger, IMapper mapper) : IRequestHandler<CreateCommentCommand, Result<CommentResponse>>
    {
        public async Task<Result<CommentResponse>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
        {

            var user = await userAccessor.GetCurrentUser();
            if (user is null)
            {
                logger.LogDebug("User Not Found");
                return new Result<CommentResponse>(new AppException(HttpStatusCode.Unauthorized, "User Not Found"));
            }

            logger.LogDebug("User with id {userId} was found", user.Id);
            logger.LogDebug("Name: {name}", user.UserName);
            logger.LogDebug("Email: {email}", user.Email);
            logger.LogDebug("Id: {id}", user.Id);

            var request = command.Request;


            dbContext.BeginTransaction();

            try
            {
                var comment = mapper.Map<Comment>(request);

                await dbContext.InsertCommentAsync(comment);
                
                await dbContext.CommitAsync();
                return new Result<CommentResponse>(new CommentResponse(comment));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while creating comment");
                await dbContext.RollbackAsync();
                return new Result<CommentResponse>(new AppException(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}
