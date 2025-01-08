using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Infrastructure.Utils;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    internal sealed class CreateHabitCommandHandler(
        IHabitsDbContext dbContext,
        IMapper mapper,
        IUserAccessor userAccessor) : IRequestHandler<CreateHabitCommand, Result<CreateHabitResponse>>
    {
        public async Task<Result<CreateHabitResponse>> Handle(CreateHabitCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<CreateHabitResponse>(new AppException(System.Net.HttpStatusCode.BadRequest, "User Not Found"));

            var userId = user.Id;
            var request = command.Request;
            var habitToInsert = mapper.Map<Habit>(request);
            habitToInsert.UserId = userId;

            var currentDate = habitToInsert.StartDate;
            while (currentDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2))
            {
                if (Conditions.IsHabitMatched(currentDate, habitToInsert))
                {
                    var currentDayResult = habitToInsert!.HabitType switch
                                           {
                                               HabitTypes.Good => new DayResult
                                               {
                                                   Progress = 0,
                                                   Status = Statuses.InProgress,
                                                   Date = currentDate
                                               },
                                               HabitTypes.Limit => new DayResult
                                               {
                                                   Progress = 0,
                                                   Status = Statuses.InProgress,
                                                   Date = currentDate
                                               },
                                               HabitTypes.Quit => new DayResult
                                               {
                                                   Status = Statuses.InProgress,
                                                   Date = currentDate
                                               },
                                               _ => throw new InvalidOperationException("Something goes wrong")
                                           };

                    habitToInsert.DayResults.Add(currentDayResult);
                }

                currentDate = currentDate.AddDays(1);
            }



            dbContext.BeginTransaction();

            try
            {
                await dbContext.InsertHabitAsync(habitToInsert);


                await dbContext.CommitAsync();
                return new Result<CreateHabitResponse>(new CreateHabitResponse(habitToInsert, user));
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<CreateHabitResponse>(new AppException(System.Net.HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}