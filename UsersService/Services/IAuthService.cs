using UsersService.DTOs;
using UsersService.Models;

namespace UsersService.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
        Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerRequest);
        Task<bool> UserExistsAsync(string username, string email);
        string GenerateJwtToken(User user);
    }
}