using Microsoft.AspNetCore.Mvc;
using pruebaAsp.Models;
using pruebaAsp.Services;

namespace pruebaAsp.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthApiController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request?.FullName == null || request.Email == null || request.Password == null)
        {
            return BadRequest(new { message = "Faltan datos requeridos." });
        }

        var result = _authService.Register(request);
        if (!result.Success)
        {
            return BadRequest(new { message = "El correo ya está registrado o el email de propietario/admin ya fue usado." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request?.Email == null || request.Password == null)
        {
            return BadRequest(new { message = "Faltan datos requeridos." });
        }

        var result = _authService.Login(request);
        if (!result.Success)
        {
            return Unauthorized(new { message = "Credenciales inválidas. Verifica tu correo y contraseña." });
        }

        return Ok(result);
    }
}
