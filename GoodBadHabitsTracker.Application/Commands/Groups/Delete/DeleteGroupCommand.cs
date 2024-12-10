using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Delete
{
    public sealed record DeleteGroupCommand(Guid Id) : ICommand<Result<bool>>;
}
