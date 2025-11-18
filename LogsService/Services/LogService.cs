using Microsoft.EntityFrameworkCore;
using LogsService.Data;
using LogsService.DTOs;
using LogsService.Models;

namespace LogsService.Services
{
    public class LogService : ILogService
    {
        private readonly LogsDbContext _context;

        public LogService(LogsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LogEntryDto>> GetAllLogsAsync()
        {
            var logs = await _context.LogEntries
                .OrderByDescending(l => l.Timestamp)
                .Take(1000) // Limit to prevent performance issues
                .ToListAsync();

            return logs.Select(MapToLogEntryDto);
        }

        public async Task<LogEntryDto?> GetLogByIdAsync(int id)
        {
            var log = await _context.LogEntries.FindAsync(id);
            return log == null ? null : MapToLogEntryDto(log);
        }

        public async Task<IEnumerable<LogEntryDto>> GetLogsByServiceAsync(string service)
        {
            var logs = await _context.LogEntries
                .Where(l => l.Service == service)
                .OrderByDescending(l => l.Timestamp)
                .Take(500)
                .ToListAsync();

            return logs.Select(MapToLogEntryDto);
        }

        public async Task<IEnumerable<LogEntryDto>> GetLogsByLevelAsync(string level)
        {
            var logs = await _context.LogEntries
                .Where(l => l.Level == level)
                .OrderByDescending(l => l.Timestamp)
                .Take(500)
                .ToListAsync();

            return logs.Select(MapToLogEntryDto);
        }

        public async Task<IEnumerable<LogEntryDto>> GetLogsByUsernameAsync(string username)
        {
            var logs = await _context.LogEntries
                .Where(l => l.Username == username)
                .OrderByDescending(l => l.Timestamp)
                .Take(500)
                .ToListAsync();

            return logs.Select(MapToLogEntryDto);
        }

        public async Task<IEnumerable<LogEntryDto>> SearchLogsAsync(LogSearchDto searchDto)
        {
            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Service))
                query = query.Where(l => l.Service == searchDto.Service);

            if (!string.IsNullOrEmpty(searchDto.Level))
                query = query.Where(l => l.Level == searchDto.Level);

            if (!string.IsNullOrEmpty(searchDto.Username))
                query = query.Where(l => l.Username == searchDto.Username);

            if (!string.IsNullOrEmpty(searchDto.SearchText))
                query = query.Where(l => l.Message.Contains(searchDto.SearchText));

            if (searchDto.FromDate.HasValue)
                query = query.Where(l => l.Timestamp >= searchDto.FromDate);

            if (searchDto.ToDate.HasValue)
                query = query.Where(l => l.Timestamp <= searchDto.ToDate);

            if (!string.IsNullOrEmpty(searchDto.RequestId))
                query = query.Where(l => l.RequestId == searchDto.RequestId);

            query = query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                         .Take(searchDto.PageSize);

            var logs = await query.OrderByDescending(l => l.Timestamp).ToListAsync();
            return logs.Select(MapToLogEntryDto);
        }

        public async Task<LogEntryDto> CreateLogAsync(CreateLogEntryDto createLogDto, string? username = null)
        {
            var logEntry = new LogEntry
            {
                Service = createLogDto.Service,
                Level = createLogDto.Level,
                Message = createLogDto.Message,
                Username = username,
                RequestId = createLogDto.RequestId,
                IpAddress = createLogDto.IpAddress,
                UserAgent = createLogDto.UserAgent,
                Exception = createLogDto.Exception,
                AdditionalData = createLogDto.AdditionalData,
                Timestamp = createLogDto.Timestamp ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();

            return MapToLogEntryDto(logEntry);
        }

        public async Task<LogSummaryDto> GetLogSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate ??= DateTime.UtcNow.AddDays(-7);
            toDate ??= DateTime.UtcNow;

            var query = _context.LogEntries
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate);

            var totalLogs = await query.CountAsync();
            var errorCount = await query.CountAsync(l => l.Level == LogLevels.Error);
            var warningCount = await query.CountAsync(l => l.Level == LogLevels.Warning);
            var infoCount = await query.CountAsync(l => l.Level == LogLevels.Info);

            var serviceCounts = await query
                .GroupBy(l => l.Service)
                .Select(g => new { Service = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Service, x => x.Count);

            var levelCounts = await query
                .GroupBy(l => l.Level)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Level, x => x.Count);

            return new LogSummaryDto
            {
                TotalLogs = totalLogs,
                ErrorCount = errorCount,
                WarningCount = warningCount,
                InfoCount = infoCount,
                ServiceCounts = serviceCounts,
                LevelCounts = levelCounts,
                FromDate = fromDate.Value,
                ToDate = toDate.Value
            };
        }

        public async Task<IEnumerable<LogStatsDto>> GetLogStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            fromDate ??= DateTime.UtcNow.AddDays(-30);
            toDate ??= DateTime.UtcNow;

            var stats = await _context.LogEntries
                .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                .GroupBy(l => new { 
                    l.Service, 
                    l.Level, 
                    Date = l.Timestamp.Date 
                })
                .Select(g => new LogStatsDto
                {
                    Service = g.Key.Service,
                    Level = g.Key.Level,
                    Count = g.Count(),
                    Date = g.Key.Date
                })
                .OrderBy(s => s.Date)
                .ThenBy(s => s.Service)
                .ThenBy(s => s.Level)
                .ToListAsync();

            return stats;
        }

        public async Task<bool> DeleteOldLogsAsync(DateTime beforeDate)
        {
            var oldLogs = await _context.LogEntries
                .Where(l => l.Timestamp < beforeDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.LogEntries.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<int> GetLogCountAsync(string? service = null, string? level = null)
        {
            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrEmpty(service))
                query = query.Where(l => l.Service == service);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(l => l.Level == level);

            return await query.CountAsync();
        }

        private static LogEntryDto MapToLogEntryDto(LogEntry logEntry)
        {
            return new LogEntryDto
            {
                Id = logEntry.Id,
                Service = logEntry.Service,
                Level = logEntry.Level,
                Message = logEntry.Message,
                Username = logEntry.Username,
                RequestId = logEntry.RequestId,
                IpAddress = logEntry.IpAddress,
                UserAgent = logEntry.UserAgent,
                Exception = logEntry.Exception,
                AdditionalData = logEntry.AdditionalData,
                Timestamp = logEntry.Timestamp,
                CreatedAt = logEntry.CreatedAt
            };
        }
    }
}