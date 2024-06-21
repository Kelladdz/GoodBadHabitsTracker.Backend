using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using FluentAssertions;

namespace GoodBadHabitsTracker.Core.Tests.Models
{
    public class UserSessionTests
    {
        private readonly DataGenerator _dataGenerator;
        public UserSessionTests()
        {
            _dataGenerator = new DataGenerator();
        }
        [Fact]
        public void ToString_ReturnsCorrectString()
        {
            //ARRANGE
            var user = _dataGenerator.SeedUserSession();

            var expectedString = $"Id: {user.Id}, Name: {user.UserName}, Email: {user.Email}, Roles: {user.Roles}";

            //ACT
            var result = user.ToString();

            //ASSERT
            result.Should().BeEquivalentTo(expectedString);
        }
    }
}
