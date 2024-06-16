﻿using Azure.Core;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    internal sealed class Handler(
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : IRequestHandler<Command, User>
    {
        public async Task<User> Handle(Command command, CancellationToken cancellationToken)
        {
            User user = new()
            {
                Email = command.Request.Email,
                UserName = command.Request.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await userManager.CreateAsync(user, command.Request.Password);
            cancellationToken.ThrowIfCancellationRequested();
            if (!createUserResult.Succeeded) 
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create user: " + string.Join(", ", createUserResult.Errors));

            var isRoleExists = await roleManager.RoleExistsAsync("User");
            if (!isRoleExists)
            {
                var role = new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() };

                var createRoleResult = await roleManager.CreateAsync(role);
                if (!createRoleResult.Succeeded)
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to create role: " + string.Join(", ", createRoleResult.Errors));
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
                throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to add user to role: " + string.Join(", ", addToRoleResult.Errors));

            return user;
        }
    }
}
