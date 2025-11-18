using Microsoft.EntityFrameworkCore;
using LogsService.Models;

namespace LogsService.Data
{
    public class LogsDbContext : DbContext
    {
        public LogsDbContext(DbContextOptions<LogsDbContext> options) : base(options)
        {
        }

        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones para LogEntry
            modelBuilder.Entity<LogEntry>(entity =>
            {
                // Índices para optimizar consultas
                entity.HasIndex(l => l.Service)
                      .HasDatabaseName("IX_LogEntries_Service");

                entity.HasIndex(l => l.Level)
                      .HasDatabaseName("IX_LogEntries_Level");

                entity.HasIndex(l => l.Timestamp)
                      .HasDatabaseName("IX_LogEntries_Timestamp");

                entity.HasIndex(l => l.Username)
                      .HasFilter("[Username] IS NOT NULL")
                      .HasDatabaseName("IX_LogEntries_Username");

                entity.HasIndex(l => l.RequestId)
                      .HasFilter("[RequestId] IS NOT NULL")
                      .HasDatabaseName("IX_LogEntries_RequestId");

                // Valores por defecto
                entity.Property(l => l.Timestamp)
                      .HasDefaultValueSql("datetime('now')");

                entity.Property(l => l.CreatedAt)
                      .HasDefaultValueSql("datetime('now')");
            });

            // Datos de semilla para testing
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var baseTime = DateTime.UtcNow;
            
            var logEntries = new[]
            {
                new LogEntry
                {
                    Id = 1,
                    Service = ServiceNames.UsersService,
                    Level = LogLevels.Info,
                    Message = "User login successful",
                    Username = "admin",
                    RequestId = "REQ-001",
                    IpAddress = "192.168.1.100",
                    Timestamp = baseTime.AddHours(-24),
                    CreatedAt = baseTime.AddHours(-24)
                },
                new LogEntry
                {
                    Id = 2,
                    Service = ServiceNames.ProductsService,
                    Level = LogLevels.Info,
                    Message = "Product created successfully",
                    Username = "admin",
                    RequestId = "REQ-002",
                    IpAddress = "192.168.1.100",
                    AdditionalData = "ProductId: 1",
                    Timestamp = baseTime.AddHours(-20),
                    CreatedAt = baseTime.AddHours(-20)
                },
                new LogEntry
                {
                    Id = 3,
                    Service = ServiceNames.PaymentsService,
                    Level = LogLevels.Warning,
                    Message = "Payment processing delayed",
                    Username = "user1",
                    RequestId = "REQ-003",
                    IpAddress = "192.168.1.101",
                    AdditionalData = "PaymentId: 1, Amount: 799.99",
                    Timestamp = baseTime.AddHours(-18),
                    CreatedAt = baseTime.AddHours(-18)
                },
                new LogEntry
                {
                    Id = 4,
                    Service = ServiceNames.UsersService,
                    Level = LogLevels.Error,
                    Message = "Failed login attempt",
                    RequestId = "REQ-004",
                    IpAddress = "192.168.1.200",
                    Exception = "InvalidCredentials: Username or password incorrect",
                    Timestamp = baseTime.AddHours(-12),
                    CreatedAt = baseTime.AddHours(-12)
                },
                new LogEntry
                {
                    Id = 5,
                    Service = ServiceNames.ProductsService,
                    Level = LogLevels.Error,
                    Message = "Database connection failed",
                    RequestId = "REQ-005",
                    Exception = "SqlException: Connection timeout expired",
                    Timestamp = baseTime.AddHours(-8),
                    CreatedAt = baseTime.AddHours(-8)
                },
                new LogEntry
                {
                    Id = 6,
                    Service = ServiceNames.PaymentsService,
                    Level = LogLevels.Info,
                    Message = "Payment completed successfully",
                    Username = "user2",
                    RequestId = "REQ-006",
                    IpAddress = "192.168.1.102",
                    AdditionalData = "PaymentId: 2, TransactionId: TXN_PP_002",
                    Timestamp = baseTime.AddHours(-4),
                    CreatedAt = baseTime.AddHours(-4)
                },
                new LogEntry
                {
                    Id = 7,
                    Service = ServiceNames.LogsService,
                    Level = LogLevels.Info,
                    Message = "Log service started",
                    RequestId = "REQ-007",
                    Timestamp = baseTime.AddHours(-2),
                    CreatedAt = baseTime.AddHours(-2)
                },
                new LogEntry
                {
                    Id = 8,
                    Service = ServiceNames.UsersService,
                    Level = LogLevels.Critical,
                    Message = "Authentication service unavailable",
                    Exception = "ServiceUnavailableException: JWT validation service is down",
                    Timestamp = baseTime.AddMinutes(-30),
                    CreatedAt = baseTime.AddMinutes(-30)
                }
            };

            modelBuilder.Entity<LogEntry>().HasData(logEntries);
        }
    }
}