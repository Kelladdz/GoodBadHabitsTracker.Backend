﻿using MediatR;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    internal sealed class RegisterCommandHandler(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Email = command.Request.Email,
                UserName = command.Request.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await userManager.CreateAsync(user, command.Request.Password);
            if (!createUserResult.Succeeded) 
                throw new ValidationException(createUserResult.Errors.Select(e =>new ValidationError(e.Description.Split(' ')[0], e.Description)));

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

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            return new RegisterResponse(user, token);
        }
    }
}
