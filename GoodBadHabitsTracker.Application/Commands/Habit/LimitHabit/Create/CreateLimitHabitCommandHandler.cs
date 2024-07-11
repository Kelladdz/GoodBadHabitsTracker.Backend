using AutoMapper;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create
{
    internal sealed class CreateLimitHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, ILimitHabitsRepository limitHabitsRepository) : IRequestHandler<CreateLimitHabitCommand, LimitHabitResponse>
    {
        public async Task<LimitHabitResponse> Handle(CreateLimitHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.LimitHabit>(command.Request);
            habit.UserId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a"); //TO CHANGE

            if (!await limitHabitsRepository.CreateAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create habit");

            return new LimitHabitResponse(habit);
        }
    }
}
