using Microsoft.EntityFrameworkCore;
using UsersService.Models;

namespace UsersService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones adicionales
            modelBuilder.Entity<User>(entity =>
            {
                // Índices únicos
                entity.HasIndex(u => u.Username)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Username");

                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Email");

                // Configuraciones de propiedades para SQLite
                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(u => u.IsActive)
                      .HasDefaultValue(true);

                entity.Property(u => u.Role)
                      .HasDefaultValue("User");
            });

            // Datos de semilla (Seed Data)
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var users = new[]
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Admin",
                    FirstName = "System",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "User",
                    FirstName = "John",
                    LastName = "Doe",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    Username = "user2",
                    Email = "user2@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "User",
                    FirstName = "Jane",
                    LastName = "Smith",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    IsActive = true
                }
            };

            modelBuilder.Entity<User>().HasData(users);
        }
    }
}