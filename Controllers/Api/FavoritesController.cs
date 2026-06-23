using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pruebaAsp.Services;
using System.Security.Claims;

namespace pruebaAsp.Controllers.Api;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesApiController : ControllerBase
{
    private readonly FavoriteService _favoriteService;

    public FavoritesApiController(FavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(_favoriteService.GetFavorites(userId.Value));
    }

    [HttpPost("{propertyId}")]
    public IActionResult Add(int propertyId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var success = _favoriteService.AddFavorite(userId.Value, propertyId);
        if (!success)
        {
            return NotFound(new { message = "Property not found." });
        }

        return Ok(new { message = "Added to favorites" });
    }

    [HttpDelete("{propertyId}")]
    public IActionResult Remove(int propertyId)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var removed = _favoriteService.RemoveFavorite(userId.Value, propertyId);
        if (!removed)
        {
            return NotFound(new { message = "Favorite not found." });
        }

        return NoContent();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : null;
    }
}
