using MediatR;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    internal sealed class CreateCommandHandler(IHabitsRepository habitRepository, IMapper mapper, IUserAccessor userAccessor) : IRequestHandler<CreateCommand, HabitResponse?>
    {
        public async Task<HabitResponse?> Handle(CreateCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            var request = command.Request;
            var habitToInsert = mapper.Map<Habit>(request);

            var newHabit = await habitRepository.InsertAsync(habitToInsert, userId, cancellationToken);

            return newHabit is not null ? new HabitResponse(newHabit) : null;
        }
    }
}
