using System.ComponentModel.DataAnnotations;

namespace _0021412438_NguyenTanHuy.DTO
{
    public class AuthorDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Name { get; set; }
    }
}
