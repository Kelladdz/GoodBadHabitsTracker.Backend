using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;


namespace GoodBadHabitsTracker.Infrastructure.Utils
{
    public static class Conditions
    {
        public static bool IsHabitMatchedTodayAndDoesntHaveAnyResult(DateOnly date, Habit habit)
            => IsToday(date)
                && !habit.DayResults.Any(dayResult => dayResult.Date == date)
                && IsHabitMatched(date, habit);
        public static bool IsHabitMatchedYesterdayAndDoesntHaveAnyResult(Habit habit)
            => !habit.DayResults.Any(dayResult => dayResult.Date == DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1))
                && IsHabitMatched(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), habit);
        public static bool IsHabitMatchedYesterdayAndHasInProgressStatus(Habit habit)
            => !habit.DayResults.Any(dayResult => dayResult.Date == DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1))
                && IsHabitMatched(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), habit);
        public static bool IsHabitMatchedInPastAndDoesntHaveAnyResultOrResultIsInProgress(DateOnly date, Habit habit)
            => !IsToday(date)
                && IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(date, habit)
                && IsHabitMatched(date, habit);

        public static bool IsHabitMatchedOnStartDateDayOfWeekAndDoesntHaveAnyResultOrResultIsInProgress(DateOnly date, Habit habit)
            => IsTodayStartDateDayOfWeek(date, habit)
                && IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(date, habit)
                && IsHabitMatched(date, habit);

        public static bool IsHabitMatchedOnAnotherThanStartDateDayOfWeekAndDoesntHaveAnyResultOrResultIsInProgress(DateOnly date, Habit habit)
            => !IsTodayStartDateDayOfWeek(date, habit)
                && IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(date, habit)
                && IsHabitMatched(date, habit);

        public static bool IsHabitMatchedOnStartDateDayOfMonthkAndDoesntHaveAnyResultOrResultIsInProgress(DateOnly date, Habit habit)
            => IsTodayStartDateDayOfMonth(date, habit)
                && IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(date, habit)
                && IsHabitMatched(date, habit);

        public static bool IsHabitMatchedOnAnotherThanStartDateDayOfMonthAndDoesntHaveAnyResultOrResultIsInProgress(DateOnly date, Habit habit)
            => !IsTodayStartDateDayOfMonth(date, habit)
                && IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(date, habit)
                && IsHabitMatched(date, habit);

        public static bool IsDayInThePast(DateOnly date)
            => date < DateOnly.FromDateTime(DateTime.UtcNow);

        public static bool IsHabitMatched(DateOnly date, Habit habit)
        {
            var dayNumber = date.DayNumber;
            var dayOfWeek = date.DayOfWeek;

            return habit.StartDate.CompareTo(date) <= 0 && (habit.HabitType == HabitTypes.Good
                && ((habit.RepeatMode == RepeatModes.Daily && habit.RepeatDaysOfWeek.Contains(dayOfWeek)) ||
                   (habit.RepeatMode == RepeatModes.Monthly && habit.RepeatDaysOfMonth.Contains(date.Day)) ||
                   (habit.RepeatMode == RepeatModes.Interval && ((dayNumber - habit.StartDate.DayNumber) % habit.RepeatInterval == 0) || date == habit.StartDate)) ||
                   habit.HabitType != HabitTypes.Good);
        }
        public static bool IsRepeatModeToDailyChangeOperation(Operation operation)
            => operation.OperationType == OperationType.Replace && operation.path == "/repeatMode" && (long)operation.value == (long)RepeatModes.Daily;
        public static bool IsRepeatModeToMonthlyChangeOperation(Operation operation)
            => operation.OperationType == OperationType.Replace && operation.path == "/repeatMode" && (long)operation.value == (long)RepeatModes.Monthly;
        public static bool IsRepeatModeToIntervalChangeOperation(Operation operation)
            => operation.OperationType == OperationType.Replace && operation.path == "/repeatMode" && (long)operation.value == (long)RepeatModes.Interval;
        private static bool IsHabitDoesntHaveAnyResultOnThisDayOrResultIsInProgress(DateOnly date, Habit habit)
            => !habit.DayResults.Any(dayResult => dayResult.Date == date)
                || habit.DayResults.Any(dayResult => dayResult.Date == date && dayResult.Status == Statuses.InProgress);

        public static bool IsToday(DateOnly date)
            => date == DateOnly.FromDateTime(DateTime.Today);

        public static bool IsTodayStartDateDayOfWeek(DateOnly date, Habit habit)
            => date.DayOfWeek == habit.StartDate.DayOfWeek;

        public static bool IsTodayStartDateDayOfMonth(DateOnly date, Habit habit)
            => date.Day == habit.StartDate.Day;
        public static bool IsReplaceDayResultOperation(Operation operation, string indexOfResultToUpdate)
            => operation.OperationType == OperationType.Replace && operation.path == $"/dayResults/{indexOfResultToUpdate}";
        public static bool AreDatesDuplicated(Operation operation, IEnumerable<string> dayResultsDates)
            => dayResultsDates.Contains((string)JObject.Parse(operation.value.ToString()!)["Date"]!);
        public static bool IsProgressLowerThanQuantity(Operation operation, int quantity)
        {
#pragma warning disable CS8604 // Możliwy argument odwołania o wartości null.
            var progressFromOperation = (int?)JObject.Parse(operation.value.ToString())["Progress"];
#pragma warning restore CS8604 // Możliwy argument odwołania o wartości null.
            return progressFromOperation < quantity;
        }
        public static bool IsStatusCompleted(Operation operation)
            => (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(operation.value.ToString()!)["Status"]!.ToString()) == Statuses.Completed;
        public static bool IsRepeatDayOfWeekAddOperation(Operation operation)
            => operation.OperationType == OperationType.Add && operation.path == "/repeatDaysOfWeek/-";
        public static bool IsRepeatDayOfMonthAddOperation(Operation operation)
            => operation.OperationType == OperationType.Add && operation.path == "/repeatDaysOfMonth/-";
        public static bool IsCorrectRepeatIntervalChangeOperation(Operation operation)
            => operation.OperationType == OperationType.Replace && operation.path == "/repeatInterval" && (int)operation.value > 0 && (int)operation.value < 8 ;
    }
}
