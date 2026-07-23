using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Security
{
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        public string GenerateToken(UsersEntity user, string? audience = null, string? roles = null)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret missing.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(roles))
                claims.Add(new Claim(ClaimTypes.Role, roles));

            // Distinguishes central identity tokens (issued by /auth) from per-application
            // SSO tokens (issued by /sso), so endpoints that manage the platform itself
            // (e.g. creating applications, listing all users) can require the former.
            claims.Add(new Claim("token_type", string.IsNullOrWhiteSpace(audience) ? "central" : "sso"));

            var expirationInMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");
            var configuredAudience = string.IsNullOrWhiteSpace(audience) ? jwtSettings["Audience"] : audience;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: configuredAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}