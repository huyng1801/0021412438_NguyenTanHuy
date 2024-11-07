using System.ComponentModel.DataAnnotations;

namespace _0021412438_NguyenTanHuy.DTO
{
    public class RefreshTokenDTO
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }

}
