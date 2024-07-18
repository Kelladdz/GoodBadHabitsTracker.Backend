using AutoMapper;
using Azure;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models.Habit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Queries.Habits.GetHabits
{
    internal sealed class GetHabitsQueryHandler(
        IGoodHabitsRepository goodHabitsRepository,
        ILimitHabitsRepository limitHabitsRepository,
        IQuitHabitsRepository quitHabitsRepository,
        IMapper mapper) : IRequestHandler<GetHabitsQuery, HabitsResponse>
    {
        public async Task<HabitsResponse> Handle(GetHabitsQuery query, CancellationToken cancellationToken)
        {
            List<GoodHabit> goodHabits = new List<GoodHabit>();
            List<LimitHabit> limitHabits = new List<LimitHabit>();
            List<QuitHabit> quitHabits = new List<QuitHabit>();

            var userId = Guid.Parse("c0f91415-4590-473c-eb0f-08dc84395b6a"); //TO CHANGE

            goodHabits = await goodHabitsRepository.GetGoodHabitsAsync(query.Request.Term, query.Request.Date, userId, cancellationToken);
            limitHabits = await limitHabitsRepository.GetLimitHabitsAsync(query.Request.Term, query.Request.Date, userId, cancellationToken);
            quitHabits = await quitHabitsRepository.GetQuitHabitsAsync(query.Request.Term, query.Request.Date, userId, cancellationToken);

            var response = new HabitsResponse(goodHabits, limitHabits, quitHabits);

            return response;
        }
    }
}
