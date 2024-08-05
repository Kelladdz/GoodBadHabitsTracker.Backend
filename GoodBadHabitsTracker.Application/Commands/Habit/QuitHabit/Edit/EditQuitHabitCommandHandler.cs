using AutoMapper;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Edit;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Edit
{
    internal sealed class EditQuitHabitCommandHandler(IMapper mapper, IHttpContextAccessor httpContextAccessor, IQuitHabitsRepository quitHabitsRepository) : IRequestHandler<EditQuitHabitCommand>
    {
        public async Task Handle(EditQuitHabitCommand command, CancellationToken cancellationToken)
        {
            var habit = mapper.Map<Core.Models.Habit.QuitHabit>(command.Request);

            if (!await quitHabitsRepository.EditAsync(habit, cancellationToken))
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to edit habit");
        }
    }
}
