using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UsersService.Data;
using UsersService.DTOs;
using UsersService.Models;

namespace UsersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener perfil del usuario actual
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(MapToUserProfile(user));
        }

        /// <summary>
        /// Obtener todos los usuarios (Solo Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();

            var userProfiles = users.Select(MapToUserProfile);
            return Ok(userProfiles);
        }

        /// <summary>
        /// Obtener usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileDto>> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // Los usuarios solo pueden ver su propio perfil, excepto los admins
            if (currentUserId != id && currentUserRole != "Admin")
            {
                return Forbid();
            }

            var user = await _context.Users
                .Where(u => u.Id == id && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(MapToUserProfile(user));
        }

        /// <summary>
        /// Actualizar perfil del usuario actual
        /// </summary>
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateUserDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            // Verificar si el email ya existe (excluyendo el usuario actual)
            if (!string.IsNullOrEmpty(updateDto.Email) && 
                await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.Id != userId))
            {
                return BadRequest(new { message = "El email ya está en uso" });
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;
            
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToUserProfile(user));
        }

        /// <summary>
        /// Cambiar contraseña del usuario actual
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == userId && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            // Verificar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }

            // Actualizar contraseña
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada exitosamente" });
        }

        /// <summary>
        /// Desactivar usuario (Solo Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario desactivado exitosamente" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private static UserProfileDto MapToUserProfile(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }
    }
}