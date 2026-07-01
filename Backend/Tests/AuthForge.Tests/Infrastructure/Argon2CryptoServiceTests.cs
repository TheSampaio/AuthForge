using AuthForge.Tests.TestHelpers;
using Infrastructure.Security;

namespace AuthForge.Tests.Infrastructure
{
    public class Argon2CryptoServiceTests
    {
        private readonly Argon2CryptoService _sut = new(ConfigurationTestHelper.BuildCryptoConfiguration());

        [Fact]
        public void HashPassword_ThenVerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            var hash = _sut.HashPassword("Correct-Password-123");

            Assert.True(_sut.VerifyPassword("Correct-Password-123", hash));
        }

        [Fact]
        public void VerifyPassword_WithWrongPassword_ReturnsFalse()
        {
            var hash = _sut.HashPassword("Correct-Password-123");

            Assert.False(_sut.VerifyPassword("Wrong-Password", hash));
        }

        [Fact]
        public void HashPassword_ProducesDifferentHashesForSamePassword()
        {
            var hash1 = _sut.HashPassword("Correct-Password-123");
            var hash2 = _sut.HashPassword("Correct-Password-123");

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_WithMalformedHash_ReturnsFalse()
        {
            Assert.False(_sut.VerifyPassword("whatever", "not-a-valid-hash-format"));
        }
    }
}