using System.Net;
using Microsoft.AspNetCore.Mvc;
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
using GoodBadHabitsTracker.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]
    public class AuthController(IMediator mediator, IEmailSender emailSender) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register
            ([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new RegisterCommand(request), cancellationToken);

            var token = response!.Token;
            var user = response!.User;
            var userId = response!.User.Id;

            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedUserId = HttpUtility.UrlEncode(userId.ToString());

            if (response is not null)
            {
                var link = $"{DevelopmentPaths.CONFIRM_EMAIL_CALLBACK}?token={encodedToken}&userId={encodedUserId}";
                if (link is null)
                {
                    return BadRequest("Failed to generate confirmation link");
                }
                await emailSender.SendConfirmationLinkAsync(user, link);

                return CreatedAtAction(nameof(Register), new { id = userId }, response);
            }
            else return BadRequest("Something goes wrong");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login
            ([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new LoginCommand(request), cancellationToken);

            var userFingerprint = response.UserFingerprint;
            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;

            AppendCookies(userFingerprint, refreshToken);

            return Ok(accessToken);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            DeleteCookies();
            return NoContent();
        }

        [HttpPost("external-login")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLogin(ExternalLoginRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ExternalLoginCommand(request), cancellationToken);

            var userFingerprint = response.UserFingerprint;
            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;

            AppendCookies(userFingerprint, refreshToken);

            return Ok(new { accessToken });
        }


        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail
            ([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ConfirmEmailCommand(request), cancellationToken);

            return result.Succeeded
                ? NoContent()
                : Unauthorized(result.Errors);
        }

        [HttpPost("forget-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword
            ([FromBody] ForgetPasswordRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ForgetPasswordCommand(request), cancellationToken);

            var email = response.User.Email;
            var token = response.Token;
            var user = response.User;

            var encodedToken = HttpUtility.UrlEncode(token);
            var encodedEmail = HttpUtility.UrlEncode(email);

            if (response is not null)
            {
                var link = $"{DevelopmentPaths.RESET_PASSWORD_CALLBACK}?token={encodedToken}&email={encodedEmail}";
                if (link is null)
                {
                    return BadRequest("Failed to generate reset password link");
                }
                await emailSender.SendPasswordResetLinkAsync(user, link);
                return Ok(new { token, email });
            }
            else return BadRequest("Something goes wrong");
        }

        [HttpPatch("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ResetPasswordCommand(request), cancellationToken);
            return result.Succeeded
                ? NoContent()
                : BadRequest(result.Errors);
        }

        [HttpPost("token/refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken
            (CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies[CookieNames.REFRESH_TOKEN_COOKIE_NAME];
            var response = await mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

            var userFingerprint = response.UserFingerprint;
            var accessToken = response.AccessToken;
            var newRefreshToken = response.RefreshToken;

            Response.Cookies.Delete(CookieNames.REFRESH_TOKEN_COOKIE_NAME);
            AppendCookies(userFingerprint, newRefreshToken);

            return Ok(new { accessToken });
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExternalTokens([FromBody] GetExternalTokensRequest request, [FromQuery] string provider, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetExternalTokensQuery(request, provider), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAccountCommand(), cancellationToken);

            if (result.Succeeded)
            {
                DeleteCookies();
                return NoContent();
            }
            else return BadRequest(result.Errors);

        }

        private void AppendCookies(string userFingerprint, string refreshToken)
        {

            Response.Cookies.Append(CookieNames.USER_FINGERPRINT_COOKIE_NAME, userFingerprint, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(15),
                Path = "/",
            });
            Response.Cookies.Append(CookieNames.REFRESH_TOKEN_COOKIE_NAME, refreshToken, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = false,
                Secure = true,
                MaxAge = TimeSpan.FromDays(30),
                Path = "/",
            });
        }
        private void DeleteCookies()
        {
            if (Request.Cookies.TryGetValue(CookieNames.USER_FINGERPRINT_COOKIE_NAME, out var _ ))
            {
                Response.Cookies.Delete(CookieNames.USER_FINGERPRINT_COOKIE_NAME, new CookieOptions
                {
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                    Secure = true,
                    MaxAge = TimeSpan.FromMinutes(15),
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-2)
                });
            }
            if (Request.Cookies.TryGetValue(CookieNames.REFRESH_TOKEN_COOKIE_NAME, out var _))
            {
                Response.Cookies.Delete(CookieNames.REFRESH_TOKEN_COOKIE_NAME, new CookieOptions
                {
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = false,
                    Secure = true,
                    MaxAge = TimeSpan.FromDays(30),
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-2)
                });
            }

            return;
        }
    }
}