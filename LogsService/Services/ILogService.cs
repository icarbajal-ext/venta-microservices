using LogsService.DTOs;

namespace LogsService.Services
{
    public interface ILogService
    {
        Task<IEnumerable<LogEntryDto>> GetAllLogsAsync();
        Task<LogEntryDto?> GetLogByIdAsync(int id);
        Task<IEnumerable<LogEntryDto>> GetLogsByServiceAsync(string service);
        Task<IEnumerable<LogEntryDto>> GetLogsByLevelAsync(string level);
        Task<IEnumerable<LogEntryDto>> GetLogsByUsernameAsync(string username);
        Task<IEnumerable<LogEntryDto>> SearchLogsAsync(LogSearchDto searchDto);
        Task<LogEntryDto> CreateLogAsync(CreateLogEntryDto createLogDto, string? username = null);
        Task<LogSummaryDto> GetLogSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<LogStatsDto>> GetLogStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> DeleteOldLogsAsync(DateTime beforeDate);
        Task<int> GetLogCountAsync(string? service = null, string? level = null);
    }
}