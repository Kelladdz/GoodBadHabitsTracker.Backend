using AutoMapper;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Create
{
    internal sealed class CreateQuitHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, IQuitHabitsRepository quitHabitsRepository) : IRequestHandler<CreateQuitHabitCommand, QuitHabitResponse>
    {
        public async Task<QuitHabitResponse> Handle(CreateQuitHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.QuitHabit>(command.Request);
            habit.UserId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a"); //TO CHANGE

            if (!await quitHabitsRepository.CreateAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create habit");

            return new QuitHabitResponse(habit);
        }
    }
}
