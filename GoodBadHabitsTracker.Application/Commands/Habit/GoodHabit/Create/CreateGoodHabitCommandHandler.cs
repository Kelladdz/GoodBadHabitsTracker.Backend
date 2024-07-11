using AutoMapper;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models.Habit;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create
{
    internal sealed class CreateGoodHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, IGoodHabitsRepository goodHabitsRepository) : IRequestHandler<CreateGoodHabitCommand, GoodHabitResponse>
    {
        public async Task<GoodHabitResponse> Handle(CreateGoodHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.GoodHabit>(command.Request);
            habit.UserId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a"); //TO CHANGE

            if (!await goodHabitsRepository.CreateAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create habit");

            return new GoodHabitResponse(habit);
        }
    }
}
