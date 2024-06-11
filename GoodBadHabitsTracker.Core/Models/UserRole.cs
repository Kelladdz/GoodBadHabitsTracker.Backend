using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class UserRole
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public string Name { get; set; } = "User";
        public string NormalizedName { get; private set; } = default!;

        public void NormalizeName() => NormalizedName = Name.ToUpper();
        
    }
}
