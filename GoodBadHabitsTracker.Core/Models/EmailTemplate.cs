﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public record EmailTemplate(Guid Id, string Subject, string Body);
}
