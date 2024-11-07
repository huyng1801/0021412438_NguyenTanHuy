using _0021412438_NguyenTanHuy.Models;
using _0021412438_NguyenTanHuy.Utils;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace _0021412438_NguyenTanHuy.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Author entity
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

     
                // Seed data for authors
                entity.HasData(
                    new Author { Id = 1, Name = "J.K. Rowling" },
                    new Author { Id = 2, Name = "George R.R. Martin" },
                    new Author { Id = 3, Name = "J.R.R. Tolkien" }
                );
            });


            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PublishedYear).IsRequired();
                entity.Property(e => e.Genre).HasMaxLength(50);
                entity.Property(b => b.RowVersion)
                        .IsRowVersion();
                // Foreign key configuration for Author relationship
                entity.HasOne(b => b.Author)
                    .WithMany(a => a.Books)
                    .HasForeignKey(b => b.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Seed data for books
                entity.HasData(
                    new Book { Id = 1, Title = "Harry Potter and the Philosopher's Stone", PublishedYear = 1997, Genre = "Fantasy", AuthorId = 1 },
                    new Book { Id = 2, Title = "A Game of Thrones", PublishedYear = 1996, Genre = "Fantasy", AuthorId = 2 },
                    new Book { Id = 3, Title = "The Fellowship of the Ring", PublishedYear = 1954, Genre = "Fantasy", AuthorId = 3 },
                    new Book { Id = 4, Title = "Harry Potter and the Chamber of Secrets", PublishedYear = 1998, Genre = "Fantasy", AuthorId = 1 },
                    new Book { Id = 5, Title = "A Clash of Kings", PublishedYear = 1998, Genre = "Fantasy", AuthorId = 2 }
                );
            });

            // User entity configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);


            // Role entity configuration
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);

            // UserRole entity configuration (many-to-many)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // RefreshToken entity configuration
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.Token)
                .HasMaxLength(256)
                .IsRequired();

            // Seeding Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            // Seeding Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Email = "admin@example.com", PasswordHash = Function.GetMd5Hash("123456") },
                new User { Id = 2, Username = "user", Email = "user@example.com", PasswordHash = Function.GetMd5Hash("123456") }
            );

            // Seeding UserRoles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 }, // Admin role for admin user
                new UserRole { UserId = 2, RoleId = 2 }  // User role for regular user
            );



        }

    }
}
