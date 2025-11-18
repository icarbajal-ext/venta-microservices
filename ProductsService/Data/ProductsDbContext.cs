using Microsoft.EntityFrameworkCore;
using ProductsService.Models;

namespace ProductsService.Data
{
    public class ProductsDbContext : DbContext
    {
        public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU)
                      .IsUnique()
                      .HasFilter("[SKU] IS NOT NULL")
                      .HasDatabaseName("IX_Products_SKU");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(p => p.IsActive)
                      .HasDefaultValue(true);

                entity.Property(p => p.Stock)
                      .HasDefaultValue(0);

                // Relación con Category
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_Categories_Name");

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(c => c.IsActive)
                      .HasDefaultValue(true);
            });

            // Datos de semilla
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Categorías semilla
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Category
                {
                    Id = 2,
                    Name = "Furniture",
                    Description = "Office and home furniture",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },
                new Category
                {
                    Id = 3,
                    Name = "Books",
                    Description = "Books and educational materials",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },
                new Category
                {
                    Id = 4,
                    Name = "Clothing",
                    Description = "Apparel and accessories",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                }
            };

            modelBuilder.Entity<Category>().HasData(categories);

            // Productos semilla
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop Dell Inspiron 15",
                    Description = "15-inch laptop with Intel Core i5 processor, 8GB RAM, 256GB SSD",
                    Price = 799.99m,
                    CategoryId = 1,
                    SKU = "DELL-INS-15-001",
                    Stock = 25,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    CreatedBy = "system"
                },
                new Product
                {
                    Id = 2,
                    Name = "iPhone 15 Pro",
                    Description = "Latest iPhone with A17 Pro chip, 128GB storage, Pro camera system",
                    Price = 999.99m,
                    CategoryId = 1,
                    SKU = "APPLE-IP15-PRO-128",
                    Stock = 15,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    CreatedBy = "system"
                },
                new Product
                {
                    Id = 3,
                    Name = "Ergonomic Office Chair",
                    Description = "Comfortable ergonomic chair with lumbar support and adjustable height",
                    Price = 249.99m,
                    CategoryId = 2,
                    SKU = "CHAIR-ERG-001",
                    Stock = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    CreatedBy = "system"
                },
                new Product
                {
                    Id = 4,
                    Name = "Programming Book: Clean Code",
                    Description = "A handbook of agile software craftsmanship by Robert C. Martin",
                    Price = 35.99m,
                    CategoryId = 3,
                    SKU = "BOOK-PROG-CC",
                    Stock = 100,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    CreatedBy = "system"
                },
                new Product
                {
                    Id = 5,
                    Name = "Wireless Bluetooth Headphones",
                    Description = "Noise-canceling wireless headphones with 30-hour battery life",
                    Price = 149.99m,
                    CategoryId = 1,
                    SKU = "AUDIO-BT-WH001",
                    Stock = 60,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    CreatedBy = "system"
                }
            };

            modelBuilder.Entity<Product>().HasData(products);
        }
    }
}