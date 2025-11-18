using Microsoft.EntityFrameworkCore;
using PaymentsService.Models;

namespace PaymentsService.Data
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.OrderId)
                      .HasDatabaseName("IX_Payments_OrderId");

                entity.HasIndex(p => p.TransactionId)
                      .IsUnique()
                      .HasFilter("[TransactionId] IS NOT NULL")
                      .HasDatabaseName("IX_Payments_TransactionId");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(p => p.PaymentDate)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(p => p.Status)
                      .HasDefaultValue("Pending");

                // Relación con PaymentMethod
                entity.HasOne(p => p.PaymentMethod)
                      .WithMany(pm => pm.Payments)
                      .HasForeignKey(p => p.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuraciones para PaymentMethod
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasIndex(pm => pm.Name)
                      .IsUnique()
                      .HasDatabaseName("IX_PaymentMethods_Name");

                entity.Property(pm => pm.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(pm => pm.IsActive)
                      .HasDefaultValue(true);
            });

            // Datos de semilla
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Métodos de pago semilla
            var paymentMethods = new[]
            {
                new PaymentMethod
                {
                    Id = 1,
                    Name = "Credit Card",
                    Description = "Visa, MasterCard, American Express",
                    IsActive = true,
                    ProcessingFee = 2.9m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new PaymentMethod
                {
                    Id = 2,
                    Name = "PayPal",
                    Description = "PayPal secure payments",
                    IsActive = true,
                    ProcessingFee = 3.4m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5)
                },
                new PaymentMethod
                {
                    Id = 3,
                    Name = "Bank Transfer",
                    Description = "Direct bank transfer",
                    IsActive = true,
                    ProcessingFee = 0.5m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4)
                },
                new PaymentMethod
                {
                    Id = 4,
                    Name = "Apple Pay",
                    Description = "Apple Pay mobile payments",
                    IsActive = true,
                    ProcessingFee = 2.5m,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3)
                }
            };

            modelBuilder.Entity<PaymentMethod>().HasData(paymentMethods);

            // Pagos semilla
            var payments = new[]
            {
                new Payment
                {
                    Id = 1,
                    OrderId = 101,
                    Amount = 799.99m,
                    PaymentMethodId = 1,
                    Status = "Completed",
                    TransactionId = "TXN_CC_001",
                    Description = "Payment for Laptop Dell Inspiron 15",
                    PaymentDate = DateTime.UtcNow.AddDays(-10),
                    ProcessedAt = DateTime.UtcNow.AddDays(-10).AddMinutes(2),
                    UserId = "1",
                    Reference = "ORD-101-PAY",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Payment
                {
                    Id = 2,
                    OrderId = 102,
                    Amount = 999.99m,
                    PaymentMethodId = 2,
                    Status = "Completed",
                    TransactionId = "TXN_PP_002",
                    Description = "Payment for iPhone 15 Pro",
                    PaymentDate = DateTime.UtcNow.AddDays(-8),
                    ProcessedAt = DateTime.UtcNow.AddDays(-8).AddMinutes(1),
                    UserId = "2",
                    Reference = "ORD-102-PAY",
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Payment
                {
                    Id = 3,
                    OrderId = 103,
                    Amount = 249.99m,
                    PaymentMethodId = 3,
                    Status = "Pending",
                    Description = "Payment for Ergonomic Office Chair",
                    PaymentDate = DateTime.UtcNow.AddDays(-3),
                    UserId = "3",
                    Reference = "ORD-103-PAY",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Payment
                {
                    Id = 4,
                    OrderId = 104,
                    Amount = 149.99m,
                    PaymentMethodId = 4,
                    Status = "Failed",
                    Description = "Payment for Wireless Headphones",
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    UserId = "2",
                    Reference = "ORD-104-PAY",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            modelBuilder.Entity<Payment>().HasData(payments);
        }
    }
}