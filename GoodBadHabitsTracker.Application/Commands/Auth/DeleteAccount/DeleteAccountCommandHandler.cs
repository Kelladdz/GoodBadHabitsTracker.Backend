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
{    
    internal sealed class DeleteAccountCommandHandler(
        UserManager<User> userManager,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAccountCommand, IdentityResult>
    {
        public async Task<IdentityResult> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
        {
                var user = await userAccessor.GetCurrentUser();
                if (user is null)
                    return IdentityResult.Failed(new IdentityError { Code = "CurrentUserNotFound", Description = "Cannot find current user" });

                else return await userManager.DeleteAsync(user);     
        }
    }
}
