using Microsoft.AspNetCore.Mvc;
using UsersService.DTOs;
using UsersService.Services;

namespace UsersService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login de usuario
        /// </summary>
        /// <param name="loginRequest">Datos de login</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginRequest);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        /// <param name="registerRequest">Datos de registro</param>
        /// <returns>Token JWT y datos del usuario creado</returns>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _authService.UserExistsAsync(registerRequest.Username, registerRequest.Email))
            {
                return BadRequest(new { message = "El usuario o email ya existe" });
            }

            var result = await _authService.RegisterAsync(registerRequest);
            
            if (result == null)
            {
                return BadRequest(new { message = "Error al crear el usuario" });
            }

            return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
        }
    }
}