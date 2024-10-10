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
    internal sealed class GroupsConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.ToTable("Groups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Id)
                .HasColumnName("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();

            builder.Property(g => g.Name)
                .HasColumnName("Name")
                .HasColumnType("nvarchar(15)")
                .IsRequired();

            builder.HasOne(g => g.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(g => g.Habits)
                .WithOne(h => h.Group)
                .HasForeignKey(h => h.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
