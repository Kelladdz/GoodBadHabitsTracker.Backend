using GoodBadHabitsTracker.Core.Models.Habit;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Persistance
{
    public class HabitsDbContext : IdentityDbContext<User, UserRole, Guid>
    {
        public HabitsDbContext(DbContextOptions<HabitsDbContext> options) : base(options) {}
        public HabitsDbContext() { }

        public DbSet<GoodHabit> GoodHabits { get; set; }
        public DbSet<LimitHabit> LimitHabit { get; set; }
        public DbSet<QuitHabit> QuitHabit { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GoodHabit>(habit =>
            {
                habit.HasBaseType(typeof(Habit));
                habit.ToTable("Good Habits");
                habit.HasOne(h => h.User)
                    .WithMany(user => user.GoodHabits)
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                habit.HasOne(h => h.Group)
                    .WithMany(group => group.GoodHabits)
                    .HasForeignKey(h => h.GroupId);
                habit.HasIndex(h => h.Name);
            });

            builder.Entity<QuitHabit>(habit =>
            {
                habit.HasBaseType(typeof(Habit));
                habit.ToTable("Quit Habits");
                habit.HasOne(h => h.User)
                    .WithMany(user => user.QuitHabits)
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                habit.HasOne(h => h.Group)
                    .WithMany(group => group.QuitHabits)
                    .HasForeignKey(h => h.GroupId);
                habit.HasIndex(h => h.Name);
            });

            builder.Entity<LimitHabit>(habit =>
            {
                habit.HasBaseType(typeof(Habit));
                habit.ToTable("Limit Habits");
                habit.HasOne(h => h.User)
                    .WithMany(user => user.LimitHabits)
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                habit.HasOne(h => h.Group)
                    .WithMany(group => group.LimitHabits)
                    .HasForeignKey(h => h.GroupId);
                habit.HasIndex(h => h.Name);
            });

            builder.Entity<Habit>(habit =>
            {
                habit.HasKey(h => h.Id);
                habit.Property(h => h.Name)
                    .IsRequired();
                habit.OwnsMany(h => h.DayResults);
                habit.OwnsMany(h => h.Comments);
            });
        }
    }
}
