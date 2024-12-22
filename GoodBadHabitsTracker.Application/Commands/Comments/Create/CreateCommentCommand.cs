using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Comments.Create
{
    public record CreateCommentCommand(Guid HabitId, CreateCommentRequest Request) : ICommand<Result<CommentResponse>>;
}
