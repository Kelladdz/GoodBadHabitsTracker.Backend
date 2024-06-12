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

            builder.Entity<GoodHabit>()
                .HasBaseType(typeof(Habit));
            builder.Entity<GoodHabit>()
                .ToTable("Good Habits");
            builder.Entity<GoodHabit>()
                .HasOne(habit => habit.User)
                .WithMany(user => user.GoodHabits)
                .HasForeignKey(habit => habit.UserId)
                .OnDelete(DeleteBehavior.Restrict); ;
            builder.Entity<GoodHabit>()
                .HasOne(habit => habit.Group)
                .WithMany(group => group.GoodHabits)
                .HasForeignKey(habit => habit.GroupId);
            builder.Entity<GoodHabit>()
                .HasIndex(habit => habit.Name);


            builder.Entity<QuitHabit>()
                .HasBaseType(typeof(Habit));
            builder.Entity<QuitHabit>()
                .ToTable("Quit Habits");
            builder.Entity<QuitHabit>()
                .HasOne(habit => habit.User)
                .WithMany(user => user.QuitHabits)
                .HasForeignKey(habit => habit.UserId)
                .OnDelete(DeleteBehavior.Restrict); ;
            builder.Entity<QuitHabit>()
                .HasOne(habit => habit.Group)
                .WithMany(group => group.QuitHabits)
                .HasForeignKey(habit => habit.GroupId);
            builder.Entity<GoodHabit>()
                .HasIndex(habit => habit.Name);

            builder.Entity<LimitHabit>()
                .HasBaseType(typeof(Habit));
            builder.Entity<LimitHabit>()
                .ToTable("Limit Habits");
            builder.Entity<LimitHabit>()
                .HasOne(habit => habit.User)
                .WithMany(user => user.LimitHabits)
                .HasForeignKey(habit => habit.UserId)
                .OnDelete(DeleteBehavior.Restrict); ;
            builder.Entity<LimitHabit>()
                .HasOne(habit => habit.Group)
                .WithMany(group => group.LimitHabits)
                .HasForeignKey(habit => habit.GroupId);
            builder.Entity<GoodHabit>()
                .HasIndex(habit => habit.Name);

            builder.Entity<Habit>()
                .HasKey(habit => habit.Id);
            builder.Entity<Habit>()
                .Property(habit => habit.Name)
                .IsRequired();
            builder.Entity<Habit>()
                .OwnsMany(habit => habit.DayResults);
            builder.Entity<Habit>()
                .OwnsMany(habit => habit.Comments);
        }
    }
}
