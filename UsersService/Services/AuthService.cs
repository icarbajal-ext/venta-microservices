using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsersService.Data;
using UsersService.DTOs;
using UsersService.Models;

namespace UsersService.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                return null;
            }

            // Actualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"));

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto registerRequest)
        {
            if (await UserExistsAsync(registerRequest.Username, registerRequest.Email))
            {
                return null;
            }

            var user = new User
            {
                Username = registerRequest.Username,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"));

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}