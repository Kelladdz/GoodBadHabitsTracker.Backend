using GoodBadHabitsTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using GoodBadHabitsTracker.Infrastructure.Configurations;

namespace GoodBadHabitsTracker.Infrastructure.Persistance
{
    public class HabitsDbContext : IdentityDbContext<User, UserRole, Guid>
    {
        public HabitsDbContext(DbContextOptions<HabitsDbContext> options) : base(options) {}
        public HabitsDbContext() { }

        public DbSet<Habit> Habits { get; set; }
        public DbSet<DayResult> DayResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new HabitsConfiguration());
        }
    }
}
