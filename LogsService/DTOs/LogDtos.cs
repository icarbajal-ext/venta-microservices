using System.ComponentModel.DataAnnotations;

namespace LogsService.DTOs
{
    public class LogEntryDto
    {
        public int Id { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? RequestId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Exception { get; set; }
        public string? AdditionalData { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLogEntryDto
    {
        [Required]
        [StringLength(50)]
        public string Service { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Level { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

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

        public DateTime? Timestamp { get; set; }
    }

    public class LogSearchDto
    {
        public string? Service { get; set; }
        public string? Level { get; set; }
        public string? Username { get; set; }
        public string? SearchText { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? RequestId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class LogStatsDto
    {
        public string Service { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class LogSummaryDto
    {
        public int TotalLogs { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public Dictionary<string, int> ServiceCounts { get; set; } = new();
        public Dictionary<string, int> LevelCounts { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}