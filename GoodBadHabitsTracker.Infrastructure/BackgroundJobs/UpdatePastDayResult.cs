using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Hosting;
using Quartz;
using System.Threading;


namespace GoodBadHabitsTracker.Infrastructure.BackgroundJobs
{
    internal sealed class UpdatePastDayResult(IHabitsRepository habitsRepository) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
            => await habitsRepository.UpdateAllAsync();
    }
}
