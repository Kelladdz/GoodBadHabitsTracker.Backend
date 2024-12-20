﻿using Microsoft.AspNetCore.Mvc;
using MediatR;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Commands.Auth.RefreshToken;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword;
using GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin;
using GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator, IEmailSender emailSender) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register
            ([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new RegisterCommand(request), cancellationToken);
            if (response is not null)
            {
                var link = $"https://localhost:8080/auth/confirm-email/callback?token={response.Token}&userId={response.User.Id}";
                if (link is null)
                {
                    return BadRequest("Failed to generate confirmation link");
                }
                emailSender.SendConfirmationLink(response.User, link);

                return CreatedAtAction(nameof(Register), new { id = response!.User.Id }, response);
            }
            else return BadRequest("Something goes wrong");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login
            ([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new LoginCommand(request), cancellationToken);

            Response.Cookies.Append("__Secure-Fgp", response.UserFingerprint, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(15),
            });
            Response.Cookies.Append("__Secure-Rt", response.RefreshToken, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromDays(30),
            });
            return Ok(new { accessToken = response.AccessToken.ToString(), refreshToken = response.RefreshToken });
        }

        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin(ExternalLoginRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ExternalLoginCommand(request), cancellationToken);
            return result.Succeeded
                ? Ok()
                : Unauthorized(result.Errors);
        }


        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail
            ([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ConfirmEmailCommand(request), cancellationToken);

            return result.Succeeded
                ? NoContent()
                : Unauthorized(result.Errors);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword
            ([FromBody] ForgetPasswordRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ForgetPasswordCommand(request), cancellationToken);
            if (response is not null)
            {
                var link = $"https://localhost:8080/auth/reset-password/callback?token={response.Token}&email={response.User.Email}";
                if (link is null)
                {
                    return BadRequest("Failed to generate reset password link");
                }
                emailSender.SendPasswordResetLink(response.User, link);
                return Ok(new
                {
                    token = response.Token,
                    email = response.User.Email,
                });
            }
            else return BadRequest("Something goes wrong");
        }

        [HttpPatch("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ResetPasswordCommand(request), cancellationToken);
            return result.Succeeded
                ? NoContent()
                : BadRequest(result.Errors);
        }

        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken
            ([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new RefreshTokenCommand(request, cancellationToken));

            Response.Cookies.Delete("__Secure-Rt");
            Response.Cookies.Append("__Secure-Fgp", response.UserFingerprint, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(15),
            });
            Response.Cookies.Append("__Secure-Rt", response.RefreshToken, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromDays(30),
            });

            return Ok(new { accessToken = response.AccessToken.ToString() });
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetExternalTokens([FromBody] GetExternalTokensRequest request, [FromQuery] string provider, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetExternalTokensQuery(request, provider), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                error => BadRequest(error));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAccountCommand(), cancellationToken);

            return result.Succeeded 
                ? NoContent() 
                : BadRequest(result.Errors);
        }
    }
}