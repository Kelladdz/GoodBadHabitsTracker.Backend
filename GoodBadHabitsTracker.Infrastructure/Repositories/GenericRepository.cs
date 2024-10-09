using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using System.Data;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;

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

        public async ValueTask<TEntity> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
        {

            var entity = await _dbContext.Set<TEntity>()
                .FindAsync(id, cancellationToken);
                
            return entity;
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
                        IconPath = habit.IconPath,
                        IsTimeBased = habit.IsTimeBased,
                        Quantity = habit.Quantity,
                        Frequency = habit.Frequency,
                        RepeatMode = habit.RepeatMode,
                        RepeatDaysOfWeek = habit.RepeatDaysOfWeek,
                        RepeatDaysOfMonth = habit.RepeatDaysOfMonth,
                        RepeatInterval = habit.RepeatInterval,
                        StartDate = habit.StartDate,    
                        UserId = userId,
                    },
                    HabitTypes.Limit => new Habit
                    {
                        Name = habit.Name,
                        HabitType = habit.HabitType,
                        IconPath = habit.IconPath,
                        IsTimeBased = habit.IsTimeBased,
                        Quantity = habit.Quantity,
                        Frequency = habit.Frequency,
                        RepeatMode = RepeatModes.NotApplicable,
                        RepeatInterval = 0,
                        StartDate = habit.StartDate,
                        UserId = userId,
                    },
                    HabitTypes.Quit => new Habit
                    {
                        Name = habit.Name,
                        HabitType = habit.HabitType,
                        IconPath = habit.IconPath,
                        IsTimeBased = false,
                        Frequency = Frequencies.NotApplicable,
                        RepeatMode = RepeatModes.NotApplicable,
                        RepeatInterval = 0,
                        StartDate = habit.StartDate,
                        UserId = userId,
                    },
                    _ => throw new InvalidOperationException("Something goes wrong")
                };


                    _dbContext.Habits.Add(newHabit);
                    return await _dbContext.SaveChangesAsync(cancellationToken) > 0
                    ? newHabit as TEntity
                    : throw new InvalidOperationException("Something goes wrong");
                
                
            }
            else throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(JsonPatchDocument<TEntity> document, Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id, cancellationToken);
                
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
