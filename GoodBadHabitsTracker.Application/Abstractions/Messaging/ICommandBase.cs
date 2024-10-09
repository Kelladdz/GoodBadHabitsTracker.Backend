using MediatR;

namespace GoodBadHabitsTracker.Application.Abstractions.Messaging
{
    public interface ICommand : IRequest, ICommandBase
    {
    }

    public interface ICommand<TResponse> : IRequest<TResponse>, ICommandBase
    {
    }

    public interface ICommandBase
    {
    }
}
