using System.ComponentModel.DataAnnotations;

namespace _0021412438_NguyenTanHuy.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int PublishedYear { get; set; }
        public string Genre { get; set; }
        public int AuthorId { get; set; }
        public virtual Author Author { get; set; }
        [Timestamp] // This property will be used for concurrency checks
        public byte[] RowVersion { get; set; }
    }
}
