using AuthForge.Tests.TestHelpers;
using Domain.Entities;
using Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthForge.Tests.Infrastructure
{
    public class JwtServiceTests
    {
        private readonly JwtService _sut = new(ConfigurationTestHelper.BuildJwtConfiguration());

        private static UsersEntity CreateUser() => new()
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            PasswordHash = "hashed",
            Birthdate = new DateTime(1990, 1, 1)
        };

        [Fact]
        public void GenerateToken_WithoutAudienceOrRoles_ProducesCentralToken()
        {
            var user = CreateUser();

            var token = _sut.GenerateToken(user);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal(ConfigurationTestHelper.Issuer, jwt.Issuer);
            Assert.Equal(ConfigurationTestHelper.Audience, jwt.Audiences.Single());
            Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal("central", jwt.Claims.Single(c => c.Type == "token_type").Value);
            Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Role);
        }

        [Fact]
        public void GenerateToken_WithAudienceAndRoles_ProducesSsoToken()
        {
            var user = CreateUser();

            var token = _sut.GenerateToken(user, "TodoList", "Admin");
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal("TodoList", jwt.Audiences.Single());
            Assert.Equal("Admin", jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal("sso", jwt.Claims.Single(c => c.Type == "token_type").Value);
        }

        [Fact]
        public void GenerateToken_ProducesUniqueJtiPerCall()
        {
            var user = CreateUser();

            var token1 = _sut.GenerateToken(user);
            var token2 = _sut.GenerateToken(user);

            var jti1 = new JwtSecurityTokenHandler().ReadJwtToken(token1).Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var jti2 = new JwtSecurityTokenHandler().ReadJwtToken(token2).Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            Assert.NotEqual(jti1, jti2);
        }
    }
}