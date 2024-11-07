using _0021412438_NguyenTanHuy.Data;
using _0021412438_NguyenTanHuy.DTO;
using _0021412438_NguyenTanHuy.Models;
using _0021412438_NguyenTanHuy.Utils;
using Microsoft.EntityFrameworkCore;

namespace _0021412438_NguyenTanHuy.Services
{
    public interface IAuthService
    {
        UserDTO Authenticate(LoginDTO login);
        string GenerateToken(UserDTO userDTO);
        RefreshToken GenerateRefreshToken(string Username);
        bool ValidateRefreshToken(string token);
        UserDTO GetUserFromExpiredToken(string token);
        (string Token, string RefreshToken) RefreshToken(string accessToken, string refreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public UserDTO Authenticate(LoginDTO login)
        {
            var user = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .SingleOrDefault(u => u.Username == login.Username);

            if (user == null || !Function.VerifyPasswordHash(login.Password, user.PasswordHash))
            {
                return null;
            }

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = roles
            };
        }


        public string GenerateToken(UserDTO userDTO)
        {
            return _tokenService.GenerateToken(userDTO);
        }

        public RefreshToken GenerateRefreshToken(string Username)
        {
            return _tokenService.GenerateRefreshToken(Username);
        }

        public bool ValidateRefreshToken(string token)
        {
            return _tokenService.ValidateRefreshToken(token);
        }

        public UserDTO GetUserFromExpiredToken(string token)
        {
            return _tokenService.GetUserFromExpiredToken(token);
        }

        public (string Token, string RefreshToken) RefreshToken(string accessToken, string refreshToken)
        {
            return _tokenService.RefreshToken(accessToken, refreshToken);
        }
    }

}
