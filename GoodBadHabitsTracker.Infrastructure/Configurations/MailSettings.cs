using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    public class MailSettings
    {
        public string Email { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Host { get; set; } = default!;    
        public int Port { get; set; }
    }
}
