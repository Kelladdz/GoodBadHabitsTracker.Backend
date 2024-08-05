using AutoMapper;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Edit;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Edit
{
    internal sealed class EditLimitHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, ILimitHabitsRepository limitHabitsRepository) : IRequestHandler<EditLimitHabitCommand>
    {
        public async Task Handle(EditLimitHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.LimitHabit>(command.Request);

            if (!await limitHabitsRepository.EditAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to edit habit");
        }
    }
}
