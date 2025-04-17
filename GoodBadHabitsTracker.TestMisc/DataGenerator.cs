using Amazon.Runtime.Internal.Transform;
using Bogus;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GoodBadHabitsTracker.TestMisc
{
    public static class DataGenerator
    {
        private static readonly Random random = new();

        public static Habit SeedHabit()
        {
            var habitGenerator = new Faker<Habit>()
                .RuleFor(h => h.Id, f => f.Random.Guid())
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => h.HabitType == HabitTypes.Quit ? false : f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => h.HabitType != HabitTypes.Quit ? f.Random.Int(1, 3600) : null)
                .RuleFor(h => h.Frequency, (f, h) => h.HabitType != HabitTypes.Quit ? f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth) : Frequencies.NonApplicable)
                .RuleFor(h => h.RepeatMode, (f, h) => h.HabitType == HabitTypes.Good ? f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval) : RepeatModes.NonApplicable)
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToList() : [])
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToList() : [])
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : 0)
                .RuleFor(h => h.ReminderTimes, (f, h) => h.HabitType == HabitTypes.Good ? Enumerable.Range(0, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToList() : [])
                .RuleFor(h => h.UserId, f => f.Random.Guid())
                .RuleFor(h => h.GroupId, f => f.Random.Guid())
                .RuleFor(h => h.Comments, f => new List<Comment>())
                .RuleFor(h => h.DayResults, f => new List<DayResult>());
            var habit = habitGenerator.Generate();
            return habit;
        }

        public static List<Habit> SeedHabitsCollection(int number)
        {
            var habitGenerator = new Faker<Habit>()
                .RuleFor(h => h.Id, f => f.Random.Guid())
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => h.HabitType == HabitTypes.Quit ? false : f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => h.HabitType != HabitTypes.Quit ? f.Random.Int(1, 3600) : null)
                .RuleFor(h => h.Frequency, (f, h) => h.HabitType != HabitTypes.Quit ? f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth) : Frequencies.NonApplicable)
                .RuleFor(h => h.RepeatMode, (f, h) => h.HabitType == HabitTypes.Good ? f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval) : RepeatModes.NonApplicable)
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToList() : [])
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToList() : [])
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : 0)
                .RuleFor(h => h.ReminderTimes, (f, h) => h.HabitType == HabitTypes.Good ? Enumerable.Range(0, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToList() : []);
            var habits = habitGenerator.Generate(number);
            return habits;
        }


        public static HabitRequest SeedHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => h.HabitType == HabitTypes.Quit ? (bool?)null : f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => h.HabitType == HabitTypes.Quit ? null : f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, (f, h) => h.HabitType == HabitTypes.Quit ? null : f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, (f, h) => h.HabitType == HabitTypes.Good ? f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval) : null)
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToArray() : null)
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToArray() : null)
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : null)
                .RuleFor(h => h.ReminderTimes, (f, h) => h.HabitType == HabitTypes.Good ? Enumerable.Range(1, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray() : null);
            var habit = habitRequestGenerator.Generate();
            return habit;
        }

        public static HabitRequest SeedGoodHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => f.Random.Bool())
                .RuleFor(h => h.Quantity, f => f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval))
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToArray() : null)
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToArray() : null)
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : null)
                .RuleFor(h => h.ReminderTimes, f => Enumerable.Range(1, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray());
            var goodHabit = habitRequestGenerator.Generate();
            return goodHabit;
        }

        public static HabitRequest SeedHabitInDailyRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => f.Random.Bool())
                .RuleFor(h => h.Quantity, f => f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => RepeatModes.Daily)
                .RuleFor(h => h.RepeatDaysOfWeek, f => Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToArray())
                .RuleFor(h => h.RepeatDaysOfMonth, f =>  null)
                .RuleFor(h => h.RepeatInterval, f => null)
                .RuleFor(h => h.ReminderTimes, f => Enumerable.Range(1, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray());
            var dailyHabit = habitRequestGenerator.Generate();
            return dailyHabit;
        }

        public static HabitRequest SeedHabitInMonthlyRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => f.Random.Bool())
                .RuleFor(h => h.Quantity, f => f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => RepeatModes.Monthly)
                .RuleFor(h => h.RepeatDaysOfWeek, f => null)
                .RuleFor(h => h.RepeatDaysOfMonth, f => Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToArray())
                .RuleFor(h => h.RepeatInterval, f => null)
                .RuleFor(h => h.ReminderTimes, f => Enumerable.Range(1, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray());
            var monthlyHabit = habitRequestGenerator.Generate();
            return monthlyHabit;
        }

        public static HabitRequest SeedHabitInIntervalRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => f.Random.Bool())
                .RuleFor(h => h.Quantity, f => f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => RepeatModes.Interval)
                .RuleFor(h => h.RepeatDaysOfWeek, f => null)
                .RuleFor(h => h.RepeatDaysOfMonth, f => null)
                .RuleFor(h => h.RepeatInterval, f => f.Random.Int(2, 7))
                .RuleFor(h => h.ReminderTimes, f => Enumerable.Range(1, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToArray());
            var intervalHabit = habitRequestGenerator.Generate();
            return intervalHabit;
        }

        public static HabitRequest SeedLimitHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Limit)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => f.Random.Bool())
                .RuleFor(h => h.Quantity, f => f.Random.Int(1, 3600))
                .RuleFor(h => h.Frequency, f => f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth))
                .RuleFor(h => h.RepeatMode, f => null)
                .RuleFor(h => h.RepeatDaysOfWeek, f => null)
                .RuleFor(h => h.RepeatDaysOfMonth, f => null)
                .RuleFor(h => h.RepeatInterval, f => null)
                .RuleFor(h => h.ReminderTimes, f => null);
            var limitHabit = habitRequestGenerator.Generate();
            return limitHabit;
        }

        public static HabitRequest SeedQuitHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Quit)
                .RuleFor(h => h.IconId, f => f.UniqueIndex)
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, f => null)
                .RuleFor(h => h.Quantity, f => null)
                .RuleFor(h => h.Frequency, f => null)
                .RuleFor(h => h.RepeatMode, f => null)
                .RuleFor(h => h.RepeatDaysOfWeek, f => null)
                .RuleFor(h => h.RepeatDaysOfMonth, f => null)
                .RuleFor(h => h.RepeatInterval, f => null)
                .RuleFor(h => h.ReminderTimes, f => null);
            var quitHabit = habitRequestGenerator.Generate();
            return quitHabit;
        }

        public static Group SeedGroup()
        {
            var habitRequestGenerator = new Faker<Group>()
                .RuleFor(g => g.Id, f => f.Random.Guid())
                .RuleFor(g => g.Name, f => f.Name.JobTitle())
                .RuleFor(g => g.UserId, f => f.Random.Guid())
                .RuleFor(g => g.Habits, f => []);
            var group = habitRequestGenerator.Generate();
            return group;
        }

        public static JsonPatchDocument SeedHabitJsonPatchDocument()
        {
            var jsonPatchDocumentGenerator = new Faker<JsonPatchDocument>()
                .RuleFor(h => h.Operations, (f, h) =>
                {

                    var operation = new Operation()
                    {
                        path = "/name",
                        op = "replace",
                        value = f.Name.JobTitle()
                    };
                    h.Operations.Add(operation);

                    return h.Operations;
                });

            var jsonPatchDocument = jsonPatchDocumentGenerator.Generate();
            return jsonPatchDocument;
        }

        public static JsonPatchDocument SeedGroupJsonPatchDocument()
        {
            var jsonPatchDocumentGenerator = new Faker<JsonPatchDocument>()
                .RuleFor(h => h.Operations, (f, h) =>
                {

                    var operation = new Operation()
                    {
                        path = "/name",
                        op = "replace",
                        value = f.Random.String2(10)
                    };
                    h.Operations.Add(operation);

                    return h.Operations;
                });

            var jsonPatchDocument = jsonPatchDocumentGenerator.Generate();
            return jsonPatchDocument;
        }

        public static string SeedRandomString(int number)
        {
            var randomStringGenerator = new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(number));

            var randomString = (string)randomStringGenerator;
            return randomString;
        }

        public static UserSession SeedUserSession()
        {
            var userName = new Faker<string>().CustomInstantiator(f => f.Internet.UserName()).Generate();
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();

            return new UserSession(Guid.NewGuid(), userName, email, ["User"]);
        }

        public static Dictionary<string, string> SeedConfiguration()
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

        public static string SeedAccessToken()
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
                    { JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()},
                    { JwtRegisteredClaimNames.Name, f.Internet.UserName() },
                    { JwtRegisteredClaimNames.Email, f.Internet.Email() },
                    { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
                    { "roles", "User" },
                    { "userFingerprint", f.Random.String2(32) },
                    { JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "email_verified", f.Internet.Email() },
                    { "iss", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "aud", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "exp", new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds() },
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
        public static string SeedRefreshToken()
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
                    { JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()},
                    { JwtRegisteredClaimNames.Name, f.Internet.UserName() },
                    { JwtRegisteredClaimNames.Email, f.Internet.Email() },
                    { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() },
                    { "roles", "User" },
                    { JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "email_verified", f.Internet.Email() },
                    { "iss", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "aud", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "exp", new DateTimeOffset(DateTime.UtcNow.AddDays(30)).ToUnixTimeSeconds() },
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

        public static string SeedIdToken(string email)
        {
            var givenName = new Faker<string>().CustomInstantiator(f => f.Name.FirstName()).Generate();
            var familyName = new Faker<string>().CustomInstantiator(f => f.Name.LastName()).Generate();
            var name = givenName + " " + familyName;
            var headerGenerator = new Faker<JwtHeader>()
                .CustomInstantiator(f => new JwtHeader()
                {
                    { "alg", "RS256" },
                    { "typ", "JWT" },
                    { "kid", f.Random.String2(21) }
                });
            var payloadGenerator = new Faker<JwtPayload>()
                .CustomInstantiator(f => new JwtPayload()
                {
                    { "given_name", givenName },
                    { "family_name", familyName },
                    {"nickname", f.Internet.UserName() },
                    { JwtRegisteredClaimNames.Name, name },
                    {"picture", f.Internet.Avatar() },
                    { "updated_at", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
                    { JwtRegisteredClaimNames.Email, email },
                    {"email_verified", true },
                    { "iss", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { "aud", $"{f.Internet.Url}:{f.Internet.Port}" },
                    { JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() },
                    { "exp", new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds() },
                    { JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString() },
                    { JwtRegisteredClaimNames.Sid, Guid.NewGuid().ToString() }
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

        public static LoginRequest SeedLoginRequest()
        {
            var loginRequestGenerator = new Faker<LoginRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.Password, f => f.Internet.Password());

            return loginRequestGenerator.Generate();
        }
        public static RegisterRequest SeedValidRegisterRequest()
        {
            var registerRequestGenerator = new Faker<RegisterRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.UserName, f => f.Internet.UserName())
                .RuleFor(rr => rr.Password, f => f.Internet.Password())
                .RuleFor(rr => rr.ConfirmPassword, (f, rr) => rr.Password);

            return registerRequestGenerator.Generate();
        }

        public static ExternalLoginRequest SeedExternalLoginRequest()
        {
            var fakeEmail = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var externalLoginRequestGenerator = new Faker<ExternalLoginRequest>()
                .RuleFor(elr => elr.AccessToken, f => SeedAccessToken())
                .RuleFor(elr => elr.ExpiresIn, f => (int)new DateTimeOffset(DateTime.UtcNow.AddMinutes(15)).ToUnixTimeSeconds())
                .RuleFor(elr => elr.Scope, f => "openid email")
                .RuleFor(elr => elr.IdToken, f => SeedIdToken(fakeEmail))
                .RuleFor(elr => elr.RefreshToken, f => SeedRandomString(32))
                .RuleFor(elr => elr.TokenType, f => "Bearer")
                .RuleFor(elr => elr.Provider, f => "Google");

            return externalLoginRequestGenerator.Generate();
        }

        public static User SeedUser()
        {
            var userGenerator = new Faker<User>()
                .RuleFor(u => u.Id, f => f.Random.Guid())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
                .RuleFor(u => u.Habits, f => new List<Habit>())
                .RuleFor(u => u.Groups, f => new List<Group>())
                .RuleFor(u => u.ImageUrl, f => f.Internet.Avatar());

            return userGenerator.Generate();
        }

        public static ResetPasswordRequest SeedResetPasswordRequest()
        {
            var resetPasswordRequestGenerator = new Faker<ResetPasswordRequest>()
                .RuleFor(rpr => rpr.Password, f => f.Internet.Password())
                .RuleFor(rpr => rpr.Token, f => SeedRandomString(32))
                .RuleFor(rpr => rpr.Email, f => f.Internet.Email());

            return resetPasswordRequestGenerator.Generate();
        }
        public static GetExternalTokensRequest SeedGetExternalTokensRequest()
        {
            var getExternalTokensRequestGenerator = new Faker<GetExternalTokensRequest>()
                .RuleFor(getr => getr.GrantType, f => "authorization_code")
                .RuleFor(getr => getr.Code, f => f.Random.String2(32))
                .RuleFor(getr => getr.RedirectUri, f => f.Internet.Url())
                .RuleFor(getr => getr.ClientId, f => f.Random.String2(32))
                .RuleFor(getr => getr.CodeVerifier, f => f.Random.String2(32));

            return getExternalTokensRequestGenerator.Generate();
        }

        public static CreateCommentRequest SeedCreateCommentRequest()
        {
            var createCommentRequest = new Faker<CreateCommentRequest>()
                .RuleFor(ccr => ccr.Body, f => f.Random.String2(32))
                .RuleFor(ccr => ccr.Date, f => DateOnly.FromDateTime(DateTime.UtcNow));

            return createCommentRequest.Generate();
        }

        public static UpdateDayResultRequest SeedUpdateDayResultRequest()
        {
            var updateDayResultRequest = new Faker<UpdateDayResultRequest>()
                .RuleFor(udr => udr.Progress, f => f.Random.Int(0, 3600))
                .RuleFor(udr => udr.Status,
                    f => f.PickRandom(Statuses.Completed, Statuses.Failed, Statuses.Skipped,
                    Statuses.InProgress));

            return updateDayResultRequest.Generate();
        }
    }
}