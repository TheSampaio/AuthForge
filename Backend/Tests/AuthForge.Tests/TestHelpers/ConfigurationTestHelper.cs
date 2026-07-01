using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthForge.Tests.TestHelpers
{
    internal static class ConfigurationTestHelper
    {
        public const string SecretKey = "unit-test-secret-key-please-ignore-1234567890";
        public const string Issuer = "AuthForgeTests";
        public const string Audience = "AuthForgeTestClients";
        public const string ExpirationInMinutes = "60";
        public const string Pepper = "unit-test-pepper";

        public static IConfiguration BuildJwtConfiguration()
        {
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s["SecretKey"]).Returns(SecretKey);
            section.Setup(s => s["Issuer"]).Returns(Issuer);
            section.Setup(s => s["Audience"]).Returns(Audience);
            section.Setup(s => s["ExpirationInMinutes"]).Returns(ExpirationInMinutes);

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetSection("JwtSettings")).Returns(section.Object);
            configuration.Setup(c => c["JwtSettings:ExpirationInMinutes"]).Returns(ExpirationInMinutes);

            return configuration.Object;
        }

        public static IConfiguration BuildCryptoConfiguration()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c["CryptoSettings:Pepper"]).Returns(Pepper);

            return configuration.Object;
        }
    }
}