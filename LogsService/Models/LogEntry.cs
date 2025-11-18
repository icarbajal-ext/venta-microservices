using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogsService.Models
{
    [Table("LogEntries")]
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Service { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Level { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Username { get; set; }

        [StringLength(100)]
        public string? RequestId { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(2000)]
        public string? Exception { get; set; }

        [StringLength(500)]
        public string? AdditionalData { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class LogLevels
    {
        public const string Trace = "TRACE";
        public const string Debug = "DEBUG";
        public const string Info = "INFO";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Critical = "CRITICAL";

        public static readonly string[] AllLevels = { Trace, Debug, Info, Warning, Error, Critical };
    }

    public static class ServiceNames
    {
        public const string UsersService = "UsersService";
        public const string ProductsService = "ProductsService";
        public const string PaymentsService = "PaymentsService";
        public const string LogsService = "LogsService";

        public static readonly string[] AllServices = { UsersService, ProductsService, PaymentsService, LogsService };
    }
}