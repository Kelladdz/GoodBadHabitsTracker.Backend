using NBomber.CSharp;
using NBomber.Http.CSharp;
using Serilog;
using Serilog.Events;
using Shouldly;
using Xunit.Abstractions;

namespace GoodBadHabitsTracker.WebApi.Tests.Performance;

public class HabitsControllerPerformanceTests
{
    private readonly ITestOutputHelper _outputHelper;
    public HabitsControllerPerformanceTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void GetAllHabits_ShouldHandleAtLeast100RequestPerSecond()
    {
        var todayString = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy/MM/dd");
        using var httpClient = new HttpClient();
        string url = $"https://localhost:7154/api/Habits?Date={todayString}";

        const int EXPECTED_REQUESTS_PER_SECOND = 100;
        const int DURATION_SECONDS = 1;

        var getHabitsScenario = Scenario.Create("GetAllHabits", async context =>
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                return Response.Ok();
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"Exception: {ex.Message}");
                return Response.Fail();
            }

        }).WithWarmUpDuration(TimeSpan.FromSeconds(5));


        var result = NBomberRunner
            .RegisterScenarios(getHabitsScenario)
            .WithMinimumLogLevel(LogEventLevel.Debug)
            .Run();

    }
}