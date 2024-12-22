using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Comments.ReadById
{
    public record ReadCommentByIdQuery(Guid Id) : IQuery<Result<CommentResponse>>;
}
