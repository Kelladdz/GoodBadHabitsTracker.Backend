using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Security.AccessTokenHandler;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Security.Cryptography;

namespace GoodBadHabitsTracker.Infrastructure.Tests.Security.AccessTokenHandler
{
    public class HandlerTests
    {
        private readonly DataGenerator _dataGenerator;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IAccessTokenHandler _accessTokenHandler;
        private readonly IConfiguration _configuration;
        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly JwtSettings _jwtSettings;


        public HandlerTests()
        {
            _dataGenerator = new DataGenerator();
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var services = new ServiceCollection();
            services.AddOptions();
            

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(_dataGenerator.SeedConfiguration())!
                .Build();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtSettings:Key").Value!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var issuer = _configuration.GetSection("JwtSettings:Issuer").Value!;
            var audience = _configuration.GetSection("JwtSettings:Audience").Value!;

            services.Configure<JwtSettings>(options =>
            {
                options.Issuer = issuer;
                options.Audience = audience;
                options.SigningCredentials =credentials;
            });

            services.AddSingleton(_configuration);
            

            _jwtSettings = new JwtSettings()
            {
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            IOptions<JwtSettings> _jwtOptions = Options.Create(_jwtSettings);

            _accessTokenHandler = new Handler(_jwtOptions);
        }

        [Fact]
        public void GenerateAccessToken_ReturnsToken()
        {
            //ARRANGE
            var userSession = _dataGenerator.SeedUserSession();

            //ACT
            var token = _accessTokenHandler.GenerateAccessToken(userSession, out string userFingerprint);

            //ASSERT
            var jsonToken = _jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var claims = jsonToken.Claims;

            Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userSession.Id.ToString());
            Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == userSession.UserName);
            Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == userSession.Email);
            Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Jti && c.Value == _jwtSettings.Jti);
            Assert.Contains(claims, c => c.Type == "roles" && c.Value == string.Join(", ", userSession.Roles));
            Assert.Contains(claims, c => c.Type == "userFingerprint");
            Assert.Contains(claims, c => c.Type == JwtRegisteredClaimNames.Iat);
            Assert.Contains(claims, c => c.Type == "authenticationMethod" && c.Value == "EmailPassword");
        }

        [Fact]
        public void GenerateUserFingerprint__Generates()
        {
            //ACT
            var userFingerprint = _accessTokenHandler.GenerateUserFingerprint();

            //ASSERT
            userFingerprint.Should().BeAssignableTo<string>();
        }

        [Fact]
        public void GenerateUserFingerprintHash__Generates()
        {
            var userFingerprint = _dataGenerator.SeedRandomString(32);
            string expectedHash;
            using (var sha256 = SHA256.Create())
            {
                byte[] userFingerprintDigest = sha256.ComputeHash(Encoding.UTF8.GetBytes(userFingerprint));
                expectedHash = Convert.ToBase64String(userFingerprintDigest);
            }
            //ACT
            var hash = _accessTokenHandler.GenerateUserFingerprintHash(userFingerprint);

            //ASSERT
            hash.Should().Be(expectedHash);
        }
    }
}
