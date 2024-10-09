using Bogus;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;

namespace GoodBadHabitsTracker.TestMisc
{
    public sealed class DataGenerator()
    {
        private readonly Random random = new();

        public GenericResponse<Habit> SeedHabitResponse()
        {
            var habitGenerator = new Faker<Habit>()
                .RuleFor(h => h.Id, f => f.Random.Guid())
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => h.HabitType == HabitTypes.Quit ? false : f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => h.HabitType != HabitTypes.Quit ? f.Random.Int(1, 3600) : null)
                .RuleFor(h => h.Frequency, (f, h) => h.HabitType != HabitTypes.Quit ? f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth) : Frequencies.NotApplicable)
                .RuleFor(h => h.RepeatMode, (f, h) => h.HabitType == HabitTypes.Good ? f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval) : RepeatModes.NotApplicable)
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToList() : [])
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToList() : [])
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : 0)
                .RuleFor(h => h.ReminderTimes, (f, h) => h.HabitType == HabitTypes.Good ? Enumerable.Range(0, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToList() : []);
            var habit = habitGenerator.Generate();
            return new GenericResponse<Habit>(habit);
        }

        public IEnumerable<GenericResponse<Habit>> SeedHabitResponseCollection()
        {
            var count = random.Next(1, 10);
            var habitGenerator = new Faker<Habit>()
                .RuleFor(h => h.Id, f => f.Random.Guid())
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
                .RuleFor(h => h.StartDate, f => f.Date.FutureDateOnly())
                .RuleFor(h => h.IsTimeBased, (f, h) => h.HabitType == HabitTypes.Quit ? false : f.Random.Bool())
                .RuleFor(h => h.Quantity, (f, h) => h.HabitType != HabitTypes.Quit ? f.Random.Int(1, 3600) : null)
                .RuleFor(h => h.Frequency, (f, h) => h.HabitType != HabitTypes.Quit ? f.PickRandom(Frequencies.PerDay, Frequencies.PerWeek, Frequencies.PerMonth) : Frequencies.NotApplicable)
                .RuleFor(h => h.RepeatMode, (f, h) => h.HabitType == HabitTypes.Good ? f.PickRandom(RepeatModes.Daily, RepeatModes.Monthly, RepeatModes.Interval) : RepeatModes.NotApplicable)
                .RuleFor(h => h.RepeatDaysOfWeek, (f, h) => h.RepeatMode == RepeatModes.Daily ? Enumerable.Range(1, random.Next(2, 7)).Select(x => f.PickRandom(Enum.GetValues<DayOfWeek>())).ToList() : [])
                .RuleFor(h => h.RepeatDaysOfMonth, (f, h) => h.RepeatMode == RepeatModes.Monthly ? Enumerable.Range(1, random.Next(2, 28)).Select(x => f.Random.Int(1, 28)).ToList() : [])
                .RuleFor(h => h.RepeatInterval, (f, h) => h.RepeatMode == RepeatModes.Interval ? f.Random.Int(2, 7) : 0)
                .RuleFor(h => h.ReminderTimes, (f, h) => h.HabitType == HabitTypes.Good ? Enumerable.Range(0, random.Next(1, 5)).Select(x => f.Date.SoonTimeOnly()).ToList() : []);
            var habits = habitGenerator.Generate(count);

            var habitsResponse = new List<GenericResponse<Habit>>();
            foreach(var habit in habits)
            {
                habitsResponse.Add(new GenericResponse<Habit>(habit));
            }
            return habitsResponse;
        }

        public HabitRequest SeedHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => f.PickRandom(HabitTypes.Good, HabitTypes.Limit, HabitTypes.Quit))
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedGoodHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedHabitInDailyRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedHabitInMonthlyRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedHabitInIntervalRepeatModeRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Good)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedLimitHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Limit)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public HabitRequest SeedQuitHabitRequest()
        {
            var habitRequestGenerator = new Faker<HabitRequest>()
                .RuleFor(h => h.Name, f => f.Name.JobTitle())
                .RuleFor(h => h.HabitType, f => HabitTypes.Quit)
                .RuleFor(h => h.IconPath, f => f.Internet.Avatar())
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

        public JsonPatchDocument<Habit> SeedHabitJsonPatchDocument()
        {
            var jsonPatchDocumentGenerator = new Faker<JsonPatchDocument<Habit>>()
                .RuleFor(h => h.Operations, (f, h) =>
                {

                    var operation = new Operation<Habit>()
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

        public string SeedRandomString(int number)
        {
            var randomStringGenerator = new Faker<string>()
                .CustomInstantiator(f => f.Random.String2(number));

            var randomString = (string)randomStringGenerator;
            return randomString;
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

        public LoginRequest SeedLoginRequest()
        {
            var loginRequestGenerator = new Faker<LoginRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.Password, f => f.Internet.Password());

            return loginRequestGenerator.Generate();
        }
        public RegisterRequest SeedValidRegisterRequest()
        {
            var registerRequestGenerator = new Faker<RegisterRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.UserName, f => f.Internet.UserName())
                .RuleFor(rr => rr.Password, f => f.Internet.Password())
                .RuleFor(rr => rr.ConfirmPassword, (f, rr) => rr.Password);

            return registerRequestGenerator.Generate();
        }
    }
}
