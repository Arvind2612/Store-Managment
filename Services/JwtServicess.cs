using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Store_Managment.jwtservice;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Store_Managment.Services
{
     
    
        public class JwtService : IJwtService
        {
            private readonly Jwtsettings _settings;

            public JwtService(IOptions<Jwtsettings> options)
            {
                _settings = options.Value;
            }

            // Generate Access JWT
            public string GenerateAccessToken(
                int userId,
                string userName,
                string email,
                List<string> roles)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    userId.ToString()),

                new Claim(ClaimTypes.Name, userName),

                new Claim(ClaimTypes.Email, email)
            };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.SecretKey));

                var credentials = new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(
                        _settings.AccessTokenExpiryMinutes),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler()
                    .WriteToken(token);
            }

            // Generate Refresh Token
            public string GenerateRefreshToken()
            {
                var randomBytes = RandomNumberGenerator.GetBytes(64);

                return Convert.ToBase64String(randomBytes);
            }

            // Hash Refresh Token
            public string HashToken(string token)
            {
                using var sha256 = SHA256.Create();

                byte[] bytes = Encoding.UTF8.GetBytes(token);

                byte[] hash = sha256.ComputeHash(bytes);

                return Convert.ToHexString(hash);
            }
        }
    }

