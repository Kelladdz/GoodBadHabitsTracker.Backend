using MediatR;

namespace GoodBadHabitsTracker.Application.Abstractions.Messaging
{
    public interface ICommand : IRequest, IRequestBase
    {
    }

    public interface ICommand<TResponse> : IRequest<TResponse>, IRequestBase
    {
    }

    public interface IQuery : IRequest, IRequestBase
    {
    }

    public interface IQuery<TResponse> : IRequest<TResponse>, IRequestBase
    {
    }

    public interface IRequestBase
    {
    }
}
