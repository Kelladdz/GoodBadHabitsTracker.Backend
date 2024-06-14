using Bogus;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.TestMisc
{
    public class DataGenerator
    {
        public RegisterRequest SeedValidRegisterRequest()
        {
            var registerRequestGenerator = new Faker<RegisterRequest>()
                .RuleFor(rr => rr.Email, f => f.Internet.Email())
                .RuleFor(rr => rr.Name, f => f.Internet.UserName())
                .RuleFor(rr => rr.Password, f => f.Internet.Password())
                .RuleFor(rr => rr.ConfirmPassword, (f, rr) => rr.Password);

            return registerRequestGenerator.Generate();
        }
    }
}
