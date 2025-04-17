using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using GoodBadHabitsTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Application.Commands.DayResults.Update
{
    internal sealed class UpdateDayResultCommandHandler(IHabitsDbContext dbContext, IUserAccessor userAccessor, ILogger logger) : IRequestHandler<UpdateDayResultCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateDayResultCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
            {
                logger.LogDebug("User Not Found");
                return new Result<bool>(new AppException(HttpStatusCode.Unauthorized, "User Not Found"));
            }

            logger.LogDebug("User with id {userId} was found", user.Id);
            logger.LogDebug("Name: {name}", user.UserName);
            logger.LogDebug("Email: {email}", user.Email);
            logger.LogDebug("Id: {id}", user.Id);

            var userId = user.Id;
            var resultId = command.Id;
            var request = command.Request;

            dbContext.BeginTransaction();

            try
            {
                var dayResultToUpdate = await dbContext.DayResults.FindAsync(resultId, cancellationToken);

                if (dayResultToUpdate is null)
                {
                    logger.LogDebug("Day result with id {resultId} was not found", resultId);
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Day result not found"));
                }

                var habit = await dbContext.Habits.FindAsync(dayResultToUpdate.HabitId, cancellationToken);
                if (habit is null)
                {

                   logger.LogDebug("Habit not found");
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Habit not found"));
                }

                var entityValidationResult = EntityValidation(request, habit);
                return await entityValidationResult.Match(
                    Succ: async _ =>
                    {
                        logger.LogDebug("Entity are valid!");
                        dayResultToUpdate.Progress = request.Progress;
                        dayResultToUpdate.Status = request.Status;

                        dbContext.DayResults.Update(dayResultToUpdate);

                        await dbContext.CommitAsync();
                        return new Result<bool>(true);
                    },
                    Fail: async ex =>
                    {
                        await dbContext.RollbackAsync();
                        return new Result<bool>((ValidationException)ex);
                    });


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating day result with id {resultId}", resultId);
                return new Result<bool>(new AppException(HttpStatusCode.InternalServerError, "Error while updating day result"));
            }
        }

        private static Result<bool> EntityValidation(UpdateDayResultRequest request, Habit habit)
        {
            var errors = new List<ValidationError>();

            if (habit.HabitType == HabitTypes.Good && request.Progress < habit.Quantity && request.Status == Statuses.Completed)
            {
                errors.Add(new ValidationError("Progress", "For good habits, progress can't be less than habit quantity when status is complete"));
            }
            else if (habit.HabitType == HabitTypes.Good && request.Progress >= habit.Quantity && request.Status != Statuses.Completed)
            {
                errors.Add(new ValidationError("Progress", "For good habits, progress can't be equal or more than habit quantity when status isn't complete"));
            }
            else if (habit.HabitType == HabitTypes.Limit && request.Progress >= habit.Quantity && request.Status == Statuses.Completed)
            {
                errors.Add(new ValidationError("Progress", "For limit habits, progress can't be equal or more than habit quantity when status is complete"));
            }

            if (errors.Count != 0)
            {
                return new Result<bool>(new ValidationException(errors));
            }
            else return new Result<bool>(true);

        }
    }
}