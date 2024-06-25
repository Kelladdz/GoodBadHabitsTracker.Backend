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
            habit.UserId = Guid.Parse("83166a55-c2f1-44a5-2a39-08dc8bf473f7"); //TO CHANGE

            if (!await quitHabitsRepository.CreateAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create habit");

            return new QuitHabitResponse(habit);
        }
    }
}
