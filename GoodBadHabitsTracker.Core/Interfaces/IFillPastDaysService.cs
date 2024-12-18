using LanguageExt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IFillPastDaysService
    {
        Task<Result<bool>> UpdateAllAsync();
    }
}
