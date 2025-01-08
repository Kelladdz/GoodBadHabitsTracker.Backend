namespace GoodBadHabitsTracker.Core.Models;

public class Stats
{
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public int Streak { get; set; }
    public int Total { get; set; }
}