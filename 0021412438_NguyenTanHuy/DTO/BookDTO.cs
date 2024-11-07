using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _0021412438_NguyenTanHuy.DTO
{
    public class BookDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        public int PublishedYear { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre cannot exceed 50 characters")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }
        [Timestamp] // Use this attribute to mark it as a concurrency token
        public string? RowVersion { get; set; } // Add RowVersion property
                                                // Helper property to convert RowVersion to byte[]
      
    }
}
