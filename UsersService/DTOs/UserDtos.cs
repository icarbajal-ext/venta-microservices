using System.ComponentModel.DataAnnotations;

namespace UsersService.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}