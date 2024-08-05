using AutoMapper;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
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

namespace GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Edit
{
    internal sealed class EditGoodHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, IGoodHabitsRepository goodHabitsRepository) : IRequestHandler<EditGoodHabitCommand>
    {
        public async Task Handle(EditGoodHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.GoodHabit>(command.Request);

            if (!await goodHabitsRepository.EditAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to edit habit");
        }
    }
}
