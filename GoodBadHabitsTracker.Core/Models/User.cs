using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public sealed class User : IdentityUser<Guid>
    {
        public List<Habit> Habits { get; init; } = new();
        public ICollection<Group> Groups { get; init; } = new List<Group>();
#pragma warning disable CS8765 

#pragma warning restore CS8765 
        public string? ImageUrl { get; set; }
    }
}

