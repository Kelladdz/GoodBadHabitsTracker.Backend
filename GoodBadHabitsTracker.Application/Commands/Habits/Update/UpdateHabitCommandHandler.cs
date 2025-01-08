using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using System.Globalization;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;


namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    internal sealed class UpdateHabitCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor,
        IValidator<Habit> validator,
        ILogger logger) : IRequestHandler<UpdateHabitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateHabitCommand command, CancellationToken cancellationToken)
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
            var habitId = command.Id;
            var document = command.Request;

            dbContext.BeginTransaction();

            try
            {
                logger.LogDebug("Looking for habit with id: {habitId}", habitId);
                var habitToUpdate = await dbContext.ReadHabitByIdAsync(habitId);
                if (habitToUpdate is null)
                {
                    logger.LogDebug("Habit Not Found");
                    await dbContext.CommitAsync();
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Habit Not Found"));
                }
                logger.LogDebug("Habit with id {habitId} was found", habitId);

                var habitProperties = habitToUpdate.GetType().GetProperties();
                foreach (var property in habitProperties)
                {
                    logger.LogDebug("{name}: {value}", property.Name, property.GetValue(habitToUpdate));
                }

                var docValidationResult = await DocumentAndEntityValidation(dbContext, document, habitToUpdate, logger);
                return await docValidationResult.Match(
                    Succ: async _ =>
                    {
                        logger.LogDebug("Document and entity are valid!");
                        if (document.Operations.Any(Conditions.IsRepeatModeToDailyChangeOperation))
                        {
                            if (habitToUpdate.RepeatDaysOfMonth.Count != 0)
                            {
                                logger.LogDebug("Clearing repeat days of month...");
                                habitToUpdate.RepeatDaysOfMonth.Clear();
                                logger.LogDebug("Cleared!");
                            }

                            if (habitToUpdate.RepeatInterval != 0)
                            {
                                logger.LogDebug("Changing repeat interval to 0...");
                                habitToUpdate.RepeatInterval = 0;
                                logger.LogDebug("Changed!");
                            }

                        }

                        if (document.Operations.Any(Conditions.IsRepeatModeToMonthlyChangeOperation))
                        {
                            if (habitToUpdate.RepeatDaysOfWeek.Count != 0)
                            {
                                logger.LogDebug("Clearing repeat days of week...");
                                habitToUpdate.RepeatDaysOfWeek.Clear();
                                logger.LogDebug("Cleared!");
                            }

                            if (habitToUpdate.RepeatInterval != 0)
                            {
                                logger.LogDebug("Changing repeat interval to 0...");
                                habitToUpdate.RepeatInterval = 0;
                                logger.LogDebug("Changed!");
                            }
                        }

                        if (document.Operations.Any(Conditions.IsRepeatModeToIntervalChangeOperation))
                        {
                            if (habitToUpdate.RepeatDaysOfWeek.Count != 0)
                            {
                                logger.LogDebug("Clearing repeat days of week...");
                                habitToUpdate.RepeatDaysOfWeek.Clear();
                                logger.LogDebug("Cleared!");
                            }

                            if (habitToUpdate.RepeatDaysOfMonth.Count != 0)
                            {
                                logger.LogDebug("Clearing repeat days of month...");
                                habitToUpdate.RepeatDaysOfMonth.Clear();
                                logger.LogDebug("Cleared!");
                            }
                        }

                        logger.LogDebug("Applying patch document to habit...");
                        document.ApplyTo(habitToUpdate);

                        logger.LogDebug("Applied!");
                        logger.LogDebug("Validating habit...");
                        var validationResult = validator.Validate(habitToUpdate);
                        if (validationResult.IsValid)
                        {
                            logger.LogDebug("Habit is valid!");
                            logger.LogDebug("Saving changes and commiting...");

                            await dbContext.CommitAsync();
                            logger.LogDebug("Commited!");
                            return new Result<bool>(true);
                        }
                        else
                        {
                            logger.LogError("Habit is invalid!");
                            await dbContext.RollbackAsync();
                            return new Result<bool>(new Exceptions.ValidationException(validationResult.Errors.Select(err => new ValidationError(err.PropertyName, err.ErrorMessage)).ToList()));
                        }
                    },
                    Fail: async ex =>
                    {
                        await dbContext.RollbackAsync();
                        return new Result<bool>((Exceptions.ValidationException)ex);
                    });
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }


        private static async Task<Result<bool>> DocumentAndEntityValidation(IHabitsDbContext dbContext, JsonPatchDocument document, Habit habitToUpdate, ILogger logger)
        {
            logger.LogDebug("Validating document and entity...");

            var errors = new List<ValidationError>();

            if (habitToUpdate.HabitType == HabitTypes.Good)
            {
                var repeatDaysOfMonth = habitToUpdate.RepeatDaysOfMonth;
                var repeatDaysOfWeek = habitToUpdate.RepeatDaysOfWeek;
                var repeatInterval = habitToUpdate.RepeatInterval;

                var isRepeatModeToDailyChangeOperation = document.Operations.Any(Conditions.IsRepeatModeToDailyChangeOperation);
                logger.LogDebug("Repeat mode is changed to Daily: {isRepeatModeToDailyChangeOperation}", isRepeatModeToDailyChangeOperation);

                var isRepeatDayOfWeekAddOperation = document.Operations.Any(Conditions.IsRepeatDayOfWeekAddOperation);
                logger.LogDebug("Repeat days of week are added: {isRepeatDayOfWeekAddOperation}", isRepeatDayOfWeekAddOperation);

                if (isRepeatModeToDailyChangeOperation && !isRepeatDayOfWeekAddOperation)
                {
                    logger.LogError("RepeatDaysOfWeek should be added if RepeatMode is Daily");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatDaysOfWeek")!.Name, "RepeatDaysOfWeek should be added if RepeatMode is Daily"));
                }

                if ((habitToUpdate.RepeatMode != RepeatModes.Daily || !isRepeatModeToDailyChangeOperation) && isRepeatDayOfWeekAddOperation)
                {
                    logger.LogError("RepeatDaysOfWeek shouldn't be added if RepeatMode isn't Daily");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatDaysOfWeek")!.Name, "RepeatDaysOfWeek shouldn't be added if RepeatMode isn't Daily"));
                }

                var isRepeatModeToMonthlyChangeOperation = document.Operations.Any(Conditions.IsRepeatModeToMonthlyChangeOperation);
                logger.LogDebug("Repeat mode is changed to Monthly: {isRepeatModeToMonthlyChangeOperation}", isRepeatModeToMonthlyChangeOperation);

                var isRepeatDayOfMonthAddOperation = document.Operations.Any(Conditions.IsRepeatDayOfMonthAddOperation);
                logger.LogDebug("Repeat days of month are added: {isRepeatDayOfMonthAddOperation}", isRepeatDayOfMonthAddOperation);

                if (isRepeatModeToMonthlyChangeOperation && !isRepeatDayOfMonthAddOperation)
                {
                    logger.LogError("RepeatDaysOfMonth should be added if RepeatMode is Monthly");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatDaysOfMonth")!.Name, "RepeatDaysOfMonth should be added if RepeatMode is Monthly"));
                }

                if ((habitToUpdate.RepeatMode != RepeatModes.Monthly || !isRepeatModeToMonthlyChangeOperation) && isRepeatDayOfMonthAddOperation)
                {
                    logger.LogError("RepeatDaysOfMonth shouldn't be added if RepeatMode isn't Monthly");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatDaysOfMonth")!.Name, "RepeatDaysOfMonth shouldn't be added if RepeatMode isn't Monthly"));
                }

                var isRepeatModeToIntervalChangeOperation = document.Operations.Any(Conditions.IsRepeatModeToIntervalChangeOperation);
                logger.LogDebug("Repeat mode is changed to Interval: {isRepeatModeToIntervalChangeOperation}", isRepeatModeToIntervalChangeOperation);

                var isCorrectRepeatIntervalChangeOperation = document.Operations.Any(Conditions.IsCorrectRepeatIntervalChangeOperation);
                logger.LogDebug("Repeat interval is changed: {isRepeatIntervalChangeOperation}", isCorrectRepeatIntervalChangeOperation);

                if (isRepeatModeToIntervalChangeOperation && !isCorrectRepeatIntervalChangeOperation)
                {
                    logger.LogError("RepeatInterval should be between 2 and 7 if RepeatMode is Interval");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatInterval")!.Name, "RepeatInterval should be between 2 and 7 if RepeatMode is Interval"));
                }

                if ((habitToUpdate.RepeatMode != RepeatModes.Interval || !isRepeatModeToIntervalChangeOperation) && isCorrectRepeatIntervalChangeOperation)
                {
                    logger.LogError("RepeatInterval shouldn't be added if RepeatMode isn't Interval");
                    errors.Add(new ValidationError(typeof(Habit).GetProperty("RepeatInterval")!.Name, "RepeatInterval shouldn't be added if RepeatMode isn't Interval"));
                }


            }
            return errors.Count > 0
                     ? new Result<bool>(new Exceptions.ValidationException(errors))
                     : new Result<bool>(true);
        }
    }
}