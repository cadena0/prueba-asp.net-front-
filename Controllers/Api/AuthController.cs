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
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid registration request." });
        }

        var result = _authService.Register(request);
        if (!result.Success)
        {
            return BadRequest(new { message = "Email already registered or invalid role." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid login request." });
        }

        var result = _authService.Login(request);
        if (!result.Success)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(result);
    }
}
