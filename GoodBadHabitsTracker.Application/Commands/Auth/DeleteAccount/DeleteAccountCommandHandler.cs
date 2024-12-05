using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount
{    internal sealed class DeleteAccountCommandHandler(
        UserManager<User> userManager,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAccountCommand, bool>
    {
        public async Task<bool> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}
