using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register
            ([FromBody] RegisterRequest request)
        {
            if (request is null) throw new HttpRequestException("Request cannot be null.");
            User user = new()
            {
                Email = request.Email,
                UserName = request.Name,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await userManager.CreateAsync(user, request.Password!);
            if (!createUserResult.Succeeded)
            {
                return BadRequest(createUserResult.Errors);
            }

            var isRoleExists = await roleManager.RoleExistsAsync("User");
            if (!isRoleExists)
            {
                var role = new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() };
                var createRoleResult = await roleManager.CreateAsync(role);

                if (!createRoleResult.Succeeded)
                {
                    return BadRequest(createRoleResult.Errors);
                }
            }

            var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
            {
                return BadRequest(addToRoleResult.Errors);
            }

            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }
    }
}
