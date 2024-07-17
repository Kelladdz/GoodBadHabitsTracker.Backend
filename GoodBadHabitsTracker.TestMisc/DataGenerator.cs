using Bogus;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.TestMisc
{
    public class DataGenerator()
    {
        Random random = new Random();
        string[] allowedDaysOfWeek = ["Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"];

        public RegisterRequest SeedValidRegisterRequest()
        {
            var registerRequestGenerator = new Faker<RegisterRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.UserName, f => f.Internet.UserName())
                .RuleFor(rr => rr.Password, f => f.Internet.Password())
                .RuleFor(rr => rr.ConfirmPassword, (f, rr) => rr.Password);

            return registerRequestGenerator.Generate();
        }

        public LoginRequest SeedLoginRequest()
        {
            var loginRequestGenerator = new Faker<LoginRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.Password, f => f.Internet.Password());

            return loginRequestGenerator.Generate();
        }

        public string SeedAccessToken(string email)
        {
            var headerGenerator = new Faker<JwtHeader>()
                .CustomInstantiator(f => new JwtHeader()
                {
                    { "alg", "RS256" },
                    { "typ", "JWT" }
                });
            var payloadGenerator = new Faker<JwtPayload>()
                .CustomInstantiator(f => new JwtPayload()
                {
                    { JwtRegisteredClaimNames.Sub, f.Name.FirstName() },
                    { JwtRegisteredClaimNames.Name, f.Name.LastName() },
                    { JwtRegisteredClaimNames.Email, email },
                    { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
                    { "roles", "User" },
                    { "userFingerprint", f.Random.String2(32) },
                    { JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "authenticationMethod", "EmailPassword" },
                    { "email_verified", f.Internet.Email() },
                    { "iss", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "aud", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "exp", DateTime.UtcNow },
                });

            var signatureGenerator = new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(32));



            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(32))));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var header = (JwtHeader)headerGenerator;
            var payload = (JwtPayload)payloadGenerator;


            var token = new JwtSecurityToken(header, payload);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken.ToString();
        }

        public string SeedRandomString()
        {
            var tokenGenerator = new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(32));

            var token = (string)tokenGenerator;
            return token;
        }

        public UserSession SeedUserSession()
        {
            var userName = new Faker<string>().CustomInstantiator(f => f.Internet.UserName()).Generate();
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();

            return new UserSession(Guid.NewGuid(), userName, email, ["User"]);
        }

        public Dictionary<string, string> SeedConfiguration()
        {
            var configurationGenerator = new Faker<Dictionary<string, string>>()
                .CustomInstantiator(f => new Dictionary<string, string>
                {
                    { "JwtSettings:Issuer", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "JwtSettings:Audience", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "JwtSettings:Key", f.Random.String2(32) },
                });

            return configurationGenerator.Generate();
        }

        public HabitRequest SeedGoodHabitRequest()
        {
            var goodHabitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
                .RuleFor(h => h.IsGood, f => true)
                .RuleFor(h => h.IsQuit, f => null)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, f => f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => (bool)h.IsTimeBased! ? f.Random.Int(1, 60) * 60 : f.Random.Int(1, 100))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval))
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(allowedDaysOfWeek)).ToArray() : Array.Empty<string>())
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToArray() : Array.Empty<int>())
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : 0)
                .RuleFor(h => h.ReminderTimes, f => Enumerable.Range(0, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray());

            return goodHabitRequestGenerator.Generate();
        }

        public HabitRequest SeedLimitHabitRequest()
        {
            var limitHabitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
                .RuleFor(h => h.IsGood, f => false)
                .RuleFor(h => h.IsQuit, f => false)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, f => f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => (bool)h.IsTimeBased! ? f.Random.Int(1, 60) * 60 : f.Random.Int(1, 100))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth));
                

            return limitHabitRequestGenerator.Generate();
        }

        public HabitRequest SeedQuitHabitRequest()
        {
            var quitHabitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
                .RuleFor(h => h.IsGood, f => false)
                .RuleFor(h => h.IsQuit, f => true)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly());

            return quitHabitRequestGenerator.Generate();
        }

        public string SeedTooLongHabitName()
        {
            var nameGenerator = new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(101));

            return nameGenerator.Generate();
        }
    }
}
