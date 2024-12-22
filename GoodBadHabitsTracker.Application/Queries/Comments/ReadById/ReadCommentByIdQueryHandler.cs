using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Comments.ReadById
{
    internal sealed class ReadCommentByIdQueryHandler(IHabitsDbContext dbContext, IUserAccessor userAccessor) : IRequestHandler<ReadCommentByIdQuery, Result<CommentResponse>>
    {
        public async Task<Result<CommentResponse>> Handle(ReadCommentByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
            {
                return new Result<CommentResponse>(new AppException(HttpStatusCode.Unauthorized, "User Not Found"));
            }

            var commentId = query.Id;

            dbContext.BeginTransaction();

            try
            {
                var comment = await dbContext.ReadCommentByIdAsync(commentId);
                if (comment is null)
                {
                    return new Result<CommentResponse>(new AppException(HttpStatusCode.NotFound, "Comment Not Found"));
                }

                await dbContext.CommitAsync();
                return new Result<CommentResponse>(new CommentResponse(comment));
            }
            catch (Exception ex)
            {
                return new Result<CommentResponse>(new AppException(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}