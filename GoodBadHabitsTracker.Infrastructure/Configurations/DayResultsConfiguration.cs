using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    internal sealed class DayResultsConfiguration : IEntityTypeConfiguration<DayResult>
    {
        public void Configure(EntityTypeBuilder<DayResult> builder)
        {
            builder.ToTable("DayResults");

            builder.HasKey(dr => dr.Id);

            builder.Property(dr => dr.Id)
                .HasColumnName("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(dr => dr.Progress)
                .HasColumnName("Progress")
                .HasColumnType("int");

            builder.Property(dr => dr.Status)
                .HasColumnName("Status")
                .HasColumnType("int")
                .IsRequired();

            builder.Property(dr => dr.Date)
                .HasColumnName("Date")
                .HasColumnType("date")
                .IsRequired();

            builder.HasOne(dr => dr.Habit)
                .WithMany(h => h.DayResults)
                .HasForeignKey(dr => dr.HabitId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
