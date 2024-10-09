using MediatR;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    internal sealed class RegisterCommandHandler(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : IRequestHandler<RegisterCommand, User>
    {
        public async Task<User> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Email = command.Request.Email,
                UserName = command.Request.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await userManager.CreateAsync(user, command.Request.Password);
            if (!createUserResult.Succeeded) 
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create user: " + string.Join(", ", createUserResult.Errors.Select(e => e.Description)));

            var isRoleExists = await roleManager.RoleExistsAsync("User");
            if (!isRoleExists)
            {
                var role = new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() };

                var createRoleResult = await roleManager.CreateAsync(role);
                if (!createRoleResult.Succeeded)
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create role: " + string.Join(", ", createRoleResult.Errors.Select(e => e.Description)));
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to add user to role: " + string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));

            return user;
        }
    }
}
