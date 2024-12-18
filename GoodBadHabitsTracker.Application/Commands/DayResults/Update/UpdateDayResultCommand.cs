using LanguageExt.Common;
using GoodBadHabitsTracker.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.DayResults.Update
{
    public record UpdateDayResultCommand(Guid Id, UpdateDayResultRequest Request) : ICommand<Result<bool>>;
}
