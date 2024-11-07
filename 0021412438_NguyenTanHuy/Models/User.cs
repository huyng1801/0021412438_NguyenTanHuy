namespace _0021412438_NguyenTanHuy.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<UserRole> UserRoles { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
    }

}
