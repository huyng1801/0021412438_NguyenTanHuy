using _0021412438_NguyenTanHuy.DTO;
using _0021412438_NguyenTanHuy.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _0021412438_NguyenTanHuy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var userDTO = _authService.Authenticate(login);
            if (userDTO == null)
            {
                return Unauthorized();
            }

            var token = _authService.GenerateToken(userDTO);
            var refreshToken = _authService.GenerateRefreshToken(userDTO.Username);

            return Ok(new
            {
                userDTO.Id,
                userDTO.Username,
                Token = token,
                RefreshToken = refreshToken.Token,
                userDTO.Roles
            });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenDTO request)
        {
            var tokens = _authService.RefreshToken(request.Token, request.RefreshToken);
            if (tokens.Token == null || tokens.RefreshToken == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            var userDTO = _authService.GetUserFromExpiredToken(request.Token);

            return Ok(new
            {
                userDTO.Id,
                userDTO.Username,
                tokens.Token,
                tokens.RefreshToken,
                userDTO.Roles
            });
        }
    }

}
