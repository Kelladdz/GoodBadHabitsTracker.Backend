using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using System.Data;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> 
        where TEntity : class
    {
        protected HabitsDbContext _dbContext;

        public GenericRepository(HabitsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TEntity> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
        {

            if (typeof(TEntity) == typeof(Habit))
            {
                var habit = await _dbContext.Habits
                    .Include(x => x.DayResults)
                    .Include(x => x.Comments)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

                return habit as TEntity;
            }
            else if (typeof(TEntity) == typeof(Group))
            {
               var group = await _dbContext.Groups
                    .Include(x => x.Habits)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

                return group as TEntity;
            }
            else throw new NotImplementedException();
        }
            
        public async Task<IEnumerable<TEntity>> ReadAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (typeof(TEntity) == typeof(Habit))
            {
                var habits = await _dbContext.Habits
                    .Where(h => h.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return habits as IEnumerable<TEntity>;
            }
            else if (typeof(TEntity) == typeof(Group))
            {
                var groups = await _dbContext.Groups
                    .Where(g => g.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return groups as IEnumerable<TEntity>;
            }
            else throw new NotImplementedException();
        }
        public async Task<IEnumerable<TEntity>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {
            if (typeof(TEntity) == typeof(Habit))
            {
                var dayNumber = date.DayNumber;
                var dayOfWeek = date.DayOfWeek;

                var habits = await _dbContext.Habits
                    .Where(h => h.UserId == userId)
                    .AsNoTracking()
                    .AsSingleQuery()
                    .ToListAsync(cancellationToken);


                var filteredHabits = habits
                    .Where(h => (h.HabitType == HabitTypes.Good && (h.RepeatMode == RepeatModes.Daily && h.RepeatDaysOfWeek.Contains(dayOfWeek) ||
                                h.RepeatMode == RepeatModes.Monthly && h.RepeatDaysOfMonth.Contains(date.Day) ||
                                h.RepeatMode == RepeatModes.Interval && (dayNumber - h.StartDate.DayNumber) % h.RepeatInterval == 0))
                                || h.HabitType != HabitTypes.Good);
                                

                if (string.IsNullOrWhiteSpace(term))
                {
                    if (!filteredHabits.Any())
                    {
                        return [];
                    }
                    else
                    {
                        return (IEnumerable<TEntity>)filteredHabits;
                    }
                    
                }

                else
                {
                    term = term.Trim().ToLower();
                    var searchedHabits = filteredHabits.Where(h => h.Name!.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                    return (IEnumerable<TEntity>)searchedHabits;
                }
            }
            else throw new InvalidOperationException("This method is available only for Habit entity");
        }
        
        public async Task<TEntity> InsertAsync(TEntity entityToInsert, Guid userId, CancellationToken cancellationToken)
        {
            if (typeof(TEntity) == typeof(Habit))
            {
                var habit = entityToInsert as Habit;
                var newHabit = habit!.HabitType switch
                {
                    HabitTypes.Good => new Habit 
                    { 
                        Name = habit.Name,
                        HabitType = habit.HabitType,
                        IconId = habit.IconId,
                        IsTimeBased = habit.IsTimeBased,
                        Quantity = habit.Quantity,
                        Frequency = habit.Frequency,
                        RepeatMode = habit.RepeatMode,
                        RepeatDaysOfWeek = habit.RepeatDaysOfWeek,
                        RepeatDaysOfMonth = habit.RepeatDaysOfMonth,
                        RepeatInterval = habit.RepeatInterval,
                        StartDate = habit.StartDate,    
                        UserId = userId,
                        GroupId = habit.GroupId,
                    },
                    HabitTypes.Limit => new Habit
                    {
                        Name = habit.Name,
                        HabitType = habit.HabitType,
                        IconId = habit.IconId,
                        IsTimeBased = habit.IsTimeBased,
                        Quantity = habit.Quantity,
                        Frequency = habit.Frequency,
                        RepeatMode = RepeatModes.NotApplicable,
                        RepeatInterval = 0,
                        StartDate = habit.StartDate,
                        UserId = userId,
                        GroupId = habit.GroupId,
                    },
                    HabitTypes.Quit => new Habit
                    {
                        Name = habit.Name,
                        HabitType = habit.HabitType,
                        IconId = habit.IconId,
                        IsTimeBased = false,
                        Frequency = Frequencies.NotApplicable,
                        RepeatMode = RepeatModes.NotApplicable,
                        RepeatInterval = 0,
                        StartDate = habit.StartDate,
                        UserId = userId,
                        GroupId = habit.GroupId,
                    },
                    _ => throw new InvalidOperationException("Something goes wrong")
                };


                    _dbContext.Habits.Add(newHabit);
                return await _dbContext.SaveChangesAsync(cancellationToken) > 0
                ? newHabit as TEntity
                : null;
                
                
            }
            else if (typeof(TEntity) == typeof(Group))
            {
                var group = entityToInsert as Group;

                var newGroup = new Group
                {
                    Name = group!.Name,
                    UserId = userId,
                };

                _dbContext.Groups.Add(newGroup);
                return await _dbContext.SaveChangesAsync(cancellationToken) > 0
                    ? newGroup as TEntity
                    : null;
            }
            else throw new InvalidOperationException("This method is available only for Habit and Group entities");
        }

        public async Task<bool> UpdateAsync(JsonPatchDocument<TEntity> document, Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id, cancellationToken);

            if (typeof(TEntity) == typeof(Habit))
            {   var dayResultsDates = (entity as Habit)!.DayResults.Select(dayResult => dayResult.Date.ToString("o", CultureInfo.InvariantCulture)).ToList();
                
                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                   && o.path == "/dayResults/-"
                   && dayResultsDates.Contains((string)JObject.Parse(o.value.ToString()!)["Date"]!)))
                    throw new InvalidOperationException("Two day results cannot have one date, use replace operation instead");

                if ((int)Activator.CreateInstance<TEntity>().GetType().GetProperty("HabitType")!.GetValue(entity)! != 2)
                {
                    var quantity = (int)Activator.CreateInstance<TEntity>().GetType().GetProperty("Quantity")!.GetValue(entity)!;

                    if (document.Operations.Any(o => o.OperationType == OperationType.Add
                        && o.path == "/dayResults/-"
                        && (int)JObject.Parse(o.value.ToString()!)["Progress"]! < quantity
                        && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) == Statuses.Completed))
                                throw new InvalidOperationException("Progress can't be less than quantity if status is completed");

                    if (document.Operations.Any(o => o.OperationType == OperationType.Add
                        && o.path == "/dayResults/-"
                        && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                        && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                            throw new InvalidOperationException("Progress can't be more than quantity if status is not completed");

                    if (document.Operations.Any(o => o.OperationType == OperationType.Add
                        && o.path == "/dayResults/-"
                        && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                        && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                        throw new InvalidOperationException("Progress can't be more than quantity if status is not completed");
                } 

                if ((int)Activator.CreateInstance<TEntity>().GetType().GetProperty("HabitType")!.GetValue(entity)! == 0)
                {
                    var repeatDaysOfMonth = Activator.CreateInstance<TEntity>().GetType().GetProperty("RepeatDaysOfMonth")!.GetValue(entity)! as List<int?>;
                    var repeatDaysOfWeek = Activator.CreateInstance<TEntity>().GetType().GetProperty("RepeatDaysOfWeek")!.GetValue(entity)! as List<DayOfWeek?>;
                    var repeatInterval = (int)Activator.CreateInstance<TEntity>().GetType().GetProperty("RepeatInterval")!.GetValue(entity)!;

                    if (document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatMode"
                        && (int)o.value == (int)RepeatModes.Daily)
                && !document.Operations
                    .Any(o => o.OperationType == OperationType.Add
                        && o.path == "/repeatDaysOfWeek/-"))
                        throw new InvalidOperationException("RepeatDaysOfWeek should be added if RepeatMode is Daily");

                    if (document.Operations
                        .Any(o => o.OperationType == OperationType.Replace
                            && o.path == "/repeatMode"
                            && (int)o.value == (int)RepeatModes.Monthly)
                    && !document.Operations
                        .Any(o => o.OperationType == OperationType.Add
                            && o.path == "/repeatDaysOfMonth/-"))
                        throw new InvalidOperationException("RepeatDaysOfMonth should be added if RepeatMode is Monthly");

                    if (document.Operations
                        .Any(o => o.OperationType == OperationType.Replace
                            && o.path == "/repeatMode"
                            && (int)o.value == (int)RepeatModes.Interval)
                    && !document.Operations
                        .Any(o => o.OperationType == OperationType.Replace
                            && o.path == "/repeatInterval"
                            && ((int)o.value > 0 && (int)o.value < 7)))
                        throw new InvalidOperationException("RepeatDaysOfMonth should be added if RepeatMode is Monthly");

                    if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                         && o.path == "/repeatMode"
                         && (int)o.value == (int)RepeatModes.Daily))
                    {
                        if (repeatDaysOfMonth.Count != 0)
                            (entity as Habit)!.RepeatDaysOfMonth.Clear();

                        if (repeatInterval != 0)
                            (entity as Habit)!.RepeatInterval = 0;
                    }

                    if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                         && o.path == "/repeatMode"
                         && (int)o.value == (int)RepeatModes.Monthly))
                    {
                        if (repeatDaysOfWeek!.Count != 0)
                            (entity as Habit)!.RepeatDaysOfWeek.Clear();

                        if (repeatInterval != 0)
                            (entity as Habit)!.RepeatInterval = 0;
                    }

                    if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                         && o.path == "/repeatMode"
                         && (int)o.value == (int)RepeatModes.Interval))
                    {
                        if (repeatDaysOfWeek.Count != 0)
                            (entity as Habit)!.RepeatDaysOfWeek.Clear();

                        if (repeatDaysOfMonth.Count != 0)
                            (entity as Habit)!.RepeatDaysOfMonth.Clear();
                    }
                }
            }

            if (entity is null)
                return false;

            document.ApplyTo(entity);

            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id, cancellationToken);

            if (entity is null)
                return false;

            _dbContext.Set<TEntity>().Remove(entity);

            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}       
