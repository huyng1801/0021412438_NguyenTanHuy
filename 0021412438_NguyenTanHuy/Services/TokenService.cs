using _0021412438_NguyenTanHuy.Data;
using _0021412438_NguyenTanHuy.DTO;
using _0021412438_NguyenTanHuy.Models;
using _0021412438_NguyenTanHuy.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace _0021412438_NguyenTanHuy.Services
{
    public interface ITokenService
    {
        string GenerateToken(UserDTO userDTO);
        RefreshToken GenerateRefreshToken(string Username);
        bool ValidateRefreshToken(string token);
        UserDTO GetUserFromExpiredToken(string token);
        (string Token, string RefreshToken) RefreshToken(string accessToken, string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;

        public TokenService(IOptions<JwtSettings> jwtSettings, ApplicationDbContext context)
        {
            _jwtSettings = jwtSettings.Value;
            _context = context;
        }

        public string GenerateToken(UserDTO userDTO)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userDTO.Username),
        new Claim(JwtRegisteredClaimNames.Email, userDTO.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            foreach (var role in userDTO.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiresInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public RefreshToken GenerateRefreshToken(string userName)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == userName);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
            };

            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();

            return refreshToken;
        }


        public bool ValidateRefreshToken(string token)
        {
            var refreshToken = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == token);
            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.Expires <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public UserDTO GetUserFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = false, // Allow expired tokens to be validated
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key))
                }, out SecurityToken securityToken);

                var jwtToken = securityToken as JwtSecurityToken;
                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                var username = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;

                // Retrieve all roles for the user from the claims
                var roles = jwtToken.Claims
                    .Where(x => x.Type == ClaimTypes.Role)
                    .Select(x => x.Value)
                    .ToList();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || !roles.Any())
                {
                    throw new SecurityTokenException("Invalid token claims");
                }

                var user = _context.Users.SingleOrDefault(u => u.Username == username);
                if (user == null)
                {
                    throw new SecurityTokenException("User not found");
                }

                return new UserDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Roles = roles // Assign the list of roles to the UserDTO
                };
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                Debug.WriteLine($"Error: {ex.Message}");
                throw new SecurityTokenException("Token validation failed", ex);
            }
        }



        public (string Token, string RefreshToken) RefreshToken(string accessToken, string refreshToken)
        {
            if (!ValidateRefreshToken(refreshToken))
            {
                return (null, null);
            }

            var userDTO = GetUserFromExpiredToken(accessToken);

            var newJwtToken = GenerateToken(userDTO);
            var newRefreshToken = GenerateRefreshToken(userDTO.Username);

            var existingToken = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
            if (existingToken != null)
            {
                existingToken.IsRevoked = true;
                _context.RefreshTokens.Update(existingToken);
                _context.SaveChanges();
            }

            return (newJwtToken, newRefreshToken.Token);
        }

    }

}
