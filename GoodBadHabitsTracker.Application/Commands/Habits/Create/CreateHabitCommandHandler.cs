using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    internal sealed class CreateHabitCommandHandler(
        IHabitsRepository habitRepository, 
        IMapper mapper, 
        IUserAccessor userAccessor) : IRequestHandler<CreateHabitCommand, Result<CreateHabitResponse>>
    {
        public async Task<Result<CreateHabitResponse>> Handle(CreateHabitCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<CreateHabitResponse>(new AppException(System.Net.HttpStatusCode.BadRequest, "User Not Found"));
            
            var userId = user.Id;

            var request = command.Request;
            var habitToInsert = mapper.Map<Habit>(request);
            habitToInsert.UserId = userId;

            await habitRepository.InsertAsync(habitToInsert, cancellationToken);

            return new Result<CreateHabitResponse>(new CreateHabitResponse(habitToInsert, user));
        }
    }
}
