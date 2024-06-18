using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoodBadHabitsTracker.Core.Models
{
    public record UserSession(Guid Id, string UserName, string Email, IList<string> Roles)
    {
        public override string ToString() => $"Id: {Id}, Name: {UserName}, Email: {Email}, Roles: {Roles}";
    }
}
