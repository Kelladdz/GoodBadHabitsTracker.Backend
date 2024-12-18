using GoodBadHabitsTracker.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    internal sealed class CommentsConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(c => c.Body)
                .HasColumnName("Body")
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.Date)
                .HasColumnName("Date")
                .HasColumnType("date")
                .IsRequired();

            builder.HasOne(c => c.Habit)
                .WithMany(h => h.Comments)
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
