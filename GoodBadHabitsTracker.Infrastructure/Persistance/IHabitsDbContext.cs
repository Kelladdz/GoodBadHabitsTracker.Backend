using GoodBadHabitsTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Persistance
{
    public interface IHabitsDbContext
    {
        public DbSet<Habit> Habits { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<DayResult> DayResults { get; set; }
        public DbSet<Comment> Comments { get; set; }
        void SetEntryToChanged(object obj);
        void BeginTransaction();
        string GetConnectionString();
        Task CommitAsync();
        Task RollbackAsync();
        Task<Habit?> ReadHabitByIdAsync(Guid habitId, Guid userId);
        Task<IEnumerable<Habit>> ReadAllHabitsAsync(Guid userId);
        Task<IEnumerable<Habit>> SearchHabitsAsync(string term, DateOnly date, Guid userId);
        Task InsertHabitAsync(Habit habit);
        void DeleteHabit(Habit habit);
        void DeleteRange(IEnumerable<Habit> habits);
        Task<Group?> ReadGroupByIdAsync(Guid groupId, Guid userId);
        Task<IEnumerable<Group>> ReadAllGroupsAsync(Guid userId);
        Task InsertGroupAsync(Group group);
        void DeleteGroup(Group group);
        Task<DayResult?> ReadDayResultByDateAsync(Guid habitId, string date);
        Task InsertCommentAsync(Comment comment);
        Task<EmailTemplate?> ReadEmailTemplate(string templateName);
    }
}
