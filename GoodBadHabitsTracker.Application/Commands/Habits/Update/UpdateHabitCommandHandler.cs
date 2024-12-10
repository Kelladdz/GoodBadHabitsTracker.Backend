using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    internal sealed class UpdateHabitCommandHandler(IHabitsRepository habitsRepository) : IRequestHandler<UpdateHabitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateHabitCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;
            var document = command.Request;

            var habitToUpdate = await habitsRepository.FindAsync(habitId, cancellationToken);
            if (habitToUpdate == null)
                return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Habit Not Found"));

            var validationResult = DocumentAndEntityValidation(document, habitToUpdate);

            if (validationResult.IsFaulted)
                return validationResult;

            await habitsRepository.UpdateAsync(document, habitToUpdate, cancellationToken);
            return new Result<bool>(true);
        }

        private static Result<bool> DocumentAndEntityValidation(JsonPatchDocument document, Habit habitToUpdate)
        {
            var dayResultsDates = habitToUpdate.DayResults.Select(dayResult => dayResult.Date.ToString("o", CultureInfo.InvariantCulture)).ToList();

            if (document.Operations.Any(o => o.OperationType == OperationType.Add
                && o.path == "/dayResults/-"
                && dayResultsDates.Contains((string)JObject.Parse(o.value.ToString()!)["Date"]!)))
                    return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "Two day results cannot have one date, use replace operation instead"));

            if (habitToUpdate.HabitType != HabitTypes.Quit)
            {
                var quantity = habitToUpdate.Quantity;

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! < quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) == Statuses.Completed))
                        return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "Progress can't be less than quantity if status is completed"));

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                        return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "Progress can't be more than quantity if status is not completed"));

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                        return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "Progress can't be more than quantity if status is not completed"));
            }
            if (habitToUpdate.HabitType == HabitTypes.Good)
            {
                var repeatDaysOfMonth = habitToUpdate.RepeatDaysOfMonth;
                var repeatDaysOfWeek = habitToUpdate.RepeatDaysOfWeek;
                var repeatInterval = habitToUpdate.RepeatInterval;

                if (document.Operations
                .Any(o => o.OperationType == OperationType.Replace && o.path == "/repeatMode" && (int)o.value == (int)RepeatModes.Daily)
                    && !document.Operations
                .Any(o => o.OperationType == OperationType.Add && o.path == "/repeatDaysOfWeek/-"))
                    return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "RepeatDaysOfWeek should be added if RepeatMode is Daily"));

                if (document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatMode"
                        && (int)o.value == (int)RepeatModes.Monthly)
                && !document.Operations
                    .Any(o => o.OperationType == OperationType.Add
                        && o.path == "/repeatDaysOfMonth/-"))
                    return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "RepeatDaysOfMonth should be added if RepeatMode is Monthly"));

                if (document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatMode"
                        && (int)o.value == (int)RepeatModes.Interval)
                && !document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatInterval"
                        && ((int)o.value > 0 && (int)o.value < 7)))
                    return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "RepeatDaysOfMonth should be added if RepeatMode is Monthly"));

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Daily))
                {
                    if (repeatDaysOfMonth.Count != 0)
                        habitToUpdate.RepeatDaysOfMonth.Clear();

                    if (repeatInterval != 0)
                        habitToUpdate.RepeatInterval = 0;
                }

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Monthly))
                {
                    if (repeatDaysOfWeek!.Count != 0)
                        habitToUpdate.RepeatDaysOfWeek.Clear();

                    if (repeatInterval != 0)
                        habitToUpdate.RepeatInterval = 0;
                }

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Interval))
                {
                    if (repeatDaysOfWeek.Count != 0)
                        habitToUpdate.RepeatDaysOfWeek.Clear();

                    if (repeatDaysOfMonth.Count != 0)
                        habitToUpdate.RepeatDaysOfMonth.Clear();
                }
            }
            return new Result<bool>(true);
        }
    }
}
