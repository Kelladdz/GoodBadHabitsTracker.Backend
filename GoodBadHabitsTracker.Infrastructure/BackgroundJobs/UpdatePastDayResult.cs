using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Hosting;
using Quartz;
using System.Threading;


namespace GoodBadHabitsTracker.Infrastructure.BackgroundJobs
{
    internal sealed class UpdatePastDayResult(IDayResultsRepository dayResultsRepository) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        => await dayResultsRepository.UpdateAllAsync();
    }
}
