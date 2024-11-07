using _0021412438_NguyenTanHuy.DTO;
using _0021412438_NguyenTanHuy.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using _0021412438_NguyenTanHuy.Data;

namespace _0021412438_NguyenTanHuy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/books
        [HttpGet]
        public ActionResult<IEnumerable<BookDTO>> GetBooks()
        {
            var books = _context.Books.Include(b => b.Author).ToList();

            var bookDTOs = books.Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                PublishedYear = b.PublishedYear,
                Genre = b.Genre,
                AuthorId = b.Author.Id,
                AuthorName = b.Author.Name // Include AuthorName
            }).ToList();

            return Ok(bookDTOs);
        }

        // GET /api/books/{id}
        [HttpGet("{id:int}")]
        public ActionResult<BookDTO> GetBookById(int id)
        {
            var book = _context.Books.Include(b => b.Author).FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound("Book not found");

            var bookDTO = new BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                PublishedYear = book.PublishedYear,
                Genre = book.Genre,
                AuthorId = book.Author.Id,
                AuthorName = book.Author.Name // Include AuthorName
            };

            return Ok(bookDTO);
        }

        // POST /api/books
        [HttpPost]
        public ActionResult<BookDTO> CreateBook([FromBody] BookDTO newBookDTO)
        {
            if (newBookDTO.PublishedYear < 1800 || newBookDTO.PublishedYear > DateTime.Now.Year)
                return BadRequest("Published Year must be between 1800 and the current year");

            var author = _context.Authors.Find(newBookDTO.AuthorId);
            if (author == null) return NotFound("Author not found");

            var newBook = new Book
            {
                Title = newBookDTO.Title,
                PublishedYear = newBookDTO.PublishedYear,
                Genre = newBookDTO.Genre,
                Author = author
            };

            _context.Books.Add(newBook);
            _context.SaveChanges();

            newBookDTO.Id = newBook.Id;
            newBookDTO.AuthorName = author.Name; // Set AuthorName on the newBookDTO

            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBookDTO);
        }
        public static byte[] ConvertRowVersionStringToByteArray(string rowVersionString)
        {
            if (string.IsNullOrWhiteSpace(rowVersionString) || !rowVersionString.StartsWith("0x"))
            {
                throw new ArgumentException("Invalid row version string format.", nameof(rowVersionString));
            }

            // Remove the "0x" prefix
            rowVersionString = rowVersionString.Substring(2);

            // Calculate the length of the byte array (each pair of hex digits corresponds to one byte)
            int byteArrayLength = rowVersionString.Length / 2;
            byte[] byteArray = new byte[byteArrayLength];

            for (int i = 0; i < byteArrayLength; i++)
            {
                // Convert each pair of hex characters to a byte
                byteArray[i] = Convert.ToByte(rowVersionString.Substring(i * 2, 2), 16);
            }

            return byteArray;
        }


        [HttpPut("{id:int}")]
        public ActionResult<BookDTO> UpdateBook(int id, [FromBody] BookDTO updatedBookDTO)
        {

            // Convert RowVersion string to byte array
            byte[] rowVersionBytes = ConvertRowVersionStringToByteArray(updatedBookDTO.RowVersion);

            // Retrieve the current book from the database, including the RowVersion
            var book = _context.Books.AsTracking()
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound("Book not found");

            // Validate the published year
            if (updatedBookDTO.PublishedYear < 1800 || updatedBookDTO.PublishedYear > DateTime.Now.Year)
                return BadRequest("Published Year must be between 1800 and the current year");

            var author = _context.Authors.Find(updatedBookDTO.AuthorId);
            if (author == null)
                return NotFound("Author not found");

            // Check for RowVersion match
            if (!book.RowVersion.SequenceEqual(rowVersionBytes))
            {
                return Conflict(new
                {
                    Message = "The book was updated by another user. Please refresh and try again.",
                    CurrentBook = new BookDTO
                    {
                        Id = book.Id,
                        Title = book.Title,
                        PublishedYear = book.PublishedYear,
                        Genre = book.Genre,
                        AuthorId = book.AuthorId,
                        RowVersion = Convert.ToBase64String(book.RowVersion) // Return the current RowVersion as base64 string
                    }
                });
            }

            // Update book details
            book.Title = updatedBookDTO.Title;
            book.PublishedYear = updatedBookDTO.PublishedYear;
            book.Genre = updatedBookDTO.Genre;
            book.AuthorId = updatedBookDTO.AuthorId;

            try
            {
                // Save changes
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving changes.");
            }

            return Ok(updatedBookDTO);
        }


        // DELETE /api/books/{id}
        [HttpDelete("{id:int}")]
        public ActionResult DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null) return NotFound("Book not found");

            _context.Books.Remove(book);
            _context.SaveChanges();

            return NoContent();
        }

        // GET /api/books/author/{authorName}
        [HttpGet("author/{authorName}")]
        public ActionResult<IEnumerable<BookDTO>> GetBooksByAuthor(string authorName)
        {
            var booksByAuthor = _context.Books
                .Include(b => b.Author)
                .AsEnumerable() // Switch to client-side evaluation
                .Where(b => b.Author.Name.Equals(authorName, StringComparison.OrdinalIgnoreCase))
                .Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublishedYear = b.PublishedYear,
                    Genre = b.Genre,
                    AuthorId = b.Author.Id,
                    AuthorName = b.Author.Name // Include AuthorName
                })
                .ToList();

            if (!booksByAuthor.Any()) return NotFound("No books found for this author");

            return Ok(booksByAuthor);
        }
    }
}
