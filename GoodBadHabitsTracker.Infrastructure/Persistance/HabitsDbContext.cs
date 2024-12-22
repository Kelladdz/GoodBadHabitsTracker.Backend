using GoodBadHabitsTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using GoodBadHabitsTracker.Infrastructure.Utils;

namespace GoodBadHabitsTracker.Infrastructure.Persistance
{
    public class HabitsDbContext : IdentityDbContext<User, UserRole, Guid>, IHabitsDbContext
    {
        public HabitsDbContext(DbContextOptions<HabitsDbContext> options) : base(options) {}
        public HabitsDbContext() { }

        public DbSet<Habit> Habits { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DayResult> DayResults { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        private IDbContextTransaction _transaction;
        public void SetEntryToChanged(object obj)
        {
            Entry(obj).State = EntityState.Modified;
        }

        public void BeginTransaction()
        {
            _transaction = Database.BeginTransaction();
        }

        public string GetConnectionString()
        {
            return Database.GetConnectionString()!;
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
            }
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        public async Task<Habit?> ReadHabitByIdAsync(Guid habitId, Guid userId)
            => await Habits
                .Include(habit => habit.Comments)
                .Include(habit => habit.DayResults)
                .AsSplitQuery()
                .FirstOrDefaultAsync(habit => habit.Id == habitId && habit.UserId == userId);
        public async Task<IEnumerable<Habit>> ReadAllHabitsAsync(Guid userId)
        {
            var allHabits = await Habits
                .Where(habit => habit.UserId == userId)
                .ToListAsync();

            return allHabits.Count == 0 ? [] : allHabits;
        }
        public async Task<IEnumerable<Habit>> SearchHabitsAsync(string term, DateOnly date, Guid userId)
        {
            var habits = await Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.DayResults)
                .Include(h => h.Comments)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();

            if (habits is null)
                return [];

            var filteredHabits = habits
                .Where(h => Conditions.IsHabitMatched(date, h));


            if (string.IsNullOrWhiteSpace(term))
            {
                if (!filteredHabits.Any())
                    return [];

                else return filteredHabits;
            }

            else
            {
                term = term.Trim().ToLower();
                var searchedHabits = filteredHabits.Where(h => h.Name.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!searchedHabits.Any())
                    return [];

                else return searchedHabits;
            }
        }
        public async Task InsertHabitAsync(Habit habit) => await Habits.AddAsync(habit);
        public void DeleteHabit(Habit habit) => Habits.Remove(habit);
        public void DeleteRange(IEnumerable<Habit> habits) => Habits.RemoveRange(habits);
        public async Task<Group?> ReadGroupByIdAsync(Guid groupId, Guid userId)
            => await Groups
                .Include(group => group.Habits)
                .FirstOrDefaultAsync(group => group.Id == groupId && group.UserId == userId);
        public async Task<IEnumerable<Group>> ReadAllGroupsAsync(Guid userId)
        {
            var allGroups = await Groups
                .Where(habit => habit.UserId == userId)
                .ToListAsync();

            return allGroups.Count == 0 ? [] : allGroups;
        }

        public async Task InsertGroupAsync(Group group) => await Groups.AddAsync(group);
        public void DeleteGroup(Group group) => Groups.Remove(group);
        public async Task<DayResult?> ReadDayResultByDateAsync(Guid habitId, string date)
            => await DayResults
                .FirstOrDefaultAsync(dayResult => dayResult.HabitId == habitId && dayResult.Date == DateOnly.Parse(date));

        public async Task<Comment?> ReadCommentByIdAsync(Guid id) => await Comments.FindAsync(id);
        public async Task InsertCommentAsync(Comment comment) => await Comments.AddAsync(comment);
        public async Task<EmailTemplate?> ReadEmailTemplate(string templateName)
            => await EmailTemplates
                .FirstOrDefaultAsync(template => template.Subject == templateName);
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}