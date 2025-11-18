using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LogsService.DTOs;
using LogsService.Models;
using LogsService.Services;

namespace LogsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Obtener todos los logs (Solo Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LogEntryDto>>> GetLogs()
        {
            var logs = await _logService.GetAllLogsAsync();
            return Ok(logs);
        }

        /// <summary>
        /// Buscar logs con filtros
        /// </summary>
        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LogEntryDto>>> SearchLogs([FromQuery] LogSearchDto searchDto)
        {
            var currentUserRole = GetCurrentUserRole();
            
            // Los usuarios normales solo pueden ver sus propios logs
            if (currentUserRole != "Admin")
            {
                searchDto.Username = GetCurrentUsername();
            }

            var logs = await _logService.SearchLogsAsync(searchDto);
            return Ok(logs);
        }

        /// <summary>
        /// Obtener log por ID (Solo Admin)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LogEntryDto>> GetLog(int id)
        {
            var log = await _logService.GetLogByIdAsync(id);
            
            if (log == null)
                return NotFound(new { message = "Log no encontrado" });

            return Ok(log);
        }

        /// <summary>
        /// Obtener logs por servicio (Solo Admin)
        /// </summary>
        [HttpGet("service/{service}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LogEntryDto>>> GetLogsByService(string service)
        {
            if (!ServiceNames.AllServices.Contains(service))
                return BadRequest(new { message = "Servicio inválido" });

            var logs = await _logService.GetLogsByServiceAsync(service);
            return Ok(logs);
        }

        /// <summary>
        /// Obtener logs por nivel (Solo Admin)
        /// </summary>
        [HttpGet("level/{level}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LogEntryDto>>> GetLogsByLevel(string level)
        {
            if (!LogLevels.AllLevels.Contains(level.ToUpper()))
                return BadRequest(new { message = "Nivel de log inválido" });

            var logs = await _logService.GetLogsByLevelAsync(level.ToUpper());
            return Ok(logs);
        }

        /// <summary>
        /// Obtener logs del usuario actual
        /// </summary>
        [HttpGet("my-logs")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<LogEntryDto>>> GetMyLogs()
        {
            var username = GetCurrentUsername();
            var logs = await _logService.GetLogsByUsernameAsync(username);
            return Ok(logs);
        }

        /// <summary>
        /// Crear nuevo log entry
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<LogEntryDto>> CreateLog([FromBody] CreateLogEntryDto createLogDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate service name
            if (!ServiceNames.AllServices.Contains(createLogDto.Service))
                return BadRequest(new { message = "Servicio inválido" });

            // Validate log level
            if (!LogLevels.AllLevels.Contains(createLogDto.Level.ToUpper()))
                return BadRequest(new { message = "Nivel de log inválido" });

            var username = GetCurrentUsername();
            
            try
            {
                var log = await _logService.CreateLogAsync(createLogDto, username);
                return CreatedAtAction(nameof(GetLog), new { id = log.Id }, log);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener resumen de logs (Solo Admin)
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LogSummaryDto>> GetLogSummary([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var summary = await _logService.GetLogSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }

        /// <summary>
        /// Obtener estadísticas de logs (Solo Admin)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LogStatsDto>>> GetLogStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var stats = await _logService.GetLogStatsAsync(fromDate, toDate);
            return Ok(stats);
        }

        /// <summary>
        /// Obtener conteo de logs
        /// </summary>
        [HttpGet("count")]
        [Authorize]
        public async Task<ActionResult<object>> GetLogCount([FromQuery] string? service = null, [FromQuery] string? level = null)
        {
            var currentUserRole = GetCurrentUserRole();
            
            // Solo los admins pueden ver conteos globales
            if (currentUserRole != "Admin")
            {
                return Forbid();
            }

            var count = await _logService.GetLogCountAsync(service, level);
            return Ok(new { count, service, level });
        }

        /// <summary>
        /// Eliminar logs antiguos (Solo Admin)
        /// </summary>
        [HttpDelete("cleanup")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOldLogs([FromQuery] DateTime beforeDate)
        {
            if (beforeDate > DateTime.UtcNow.AddDays(-1))
                return BadRequest(new { message = "Solo se pueden eliminar logs de más de 1 día" });

            var deleted = await _logService.DeleteOldLogsAsync(beforeDate);
            
            if (deleted)
                return Ok(new { message = $"Logs anteriores a {beforeDate:yyyy-MM-dd} eliminados exitosamente" });
            else
                return Ok(new { message = "No se encontraron logs para eliminar" });
        }

        /// <summary>
        /// Obtener servicios disponibles
        /// </summary>
        [HttpGet("services")]
        public ActionResult<object> GetAvailableServices()
        {
            return Ok(new { services = ServiceNames.AllServices });
        }

        /// <summary>
        /// Obtener niveles de log disponibles
        /// </summary>
        [HttpGet("levels")]
        public ActionResult<object> GetAvailableLevels()
        {
            return Ok(new { levels = LogLevels.AllLevels });
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}