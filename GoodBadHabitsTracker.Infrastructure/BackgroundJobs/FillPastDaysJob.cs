using GoodBadHabitsTracker.Core.Interfaces;
using Quartz;

namespace GoodBadHabitsTracker.Infrastructure.BackgroundJobs
{
    internal sealed class FillPastDaysJob(IFillPastDaysService fillPastDaysService) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
             await fillPastDaysService.UpdateAllAsync();
        }
            
    }
}
