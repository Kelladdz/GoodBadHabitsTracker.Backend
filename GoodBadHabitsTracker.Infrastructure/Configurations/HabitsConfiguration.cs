using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    internal sealed class HabitsConfiguration : IEntityTypeConfiguration<Habit>
    {
        public void Configure(EntityTypeBuilder<Habit> builder)
        {

            builder.ToTable("Habits");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Id)
                .HasColumnName("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(h => h.Name)
                .HasColumnName("Name")
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            builder.Property(h => h.IconPath)
                .HasColumnName("IconPath")
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            builder.Property(h => h.HabitType)
                .HasColumnName("HabitType")
                .HasColumnType("int")   
                .IsRequired();

            builder.Property(h => h.RepeatDaysOfWeek)
                .HasColumnName("RepeatDaysOfWeek")
                .HasColumnType("nvarchar(100)");

            builder.Property(h => h.RepeatDaysOfMonth)
                .HasColumnName("RepeatDaysOfMonth")
                .HasColumnType("nvarchar(50)");

            builder.Property(h => h.StartDate)
                .HasColumnName("StartDate")
                .HasColumnType("date")
                .IsRequired();

            builder.Property(h => h.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(h => h.GroupId)
                .HasColumnName("GroupId");

            builder.Property(h => h.ReminderTimes)
                .HasColumnName("ReminderTimes")
                .HasColumnType("nvarchar(96)")
                .IsRequired();

            builder.Property(h => h.Frequency)
                .HasColumnName("Frequency")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(h => h.RepeatMode)
                .HasColumnName("RepeatMode")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(h => h.RepeatInterval)
                .HasColumnName("RepeatInterval")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(h => h.IsTimeBased)
                    .HasColumnName("IsTimeBased")
                    .HasColumnType("bit");

            builder.Property(h => h.Quantity)
                .HasColumnName("Quantity")
                .HasColumnType("int");

            builder.HasOne(h => h.User)
                .WithMany(user => user.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.Group)
                .WithMany(group => group.Habits)
                .HasForeignKey(h => h.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
