using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pruebaAsp.Models;
using pruebaAsp.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace pruebaAsp.Controllers.Api;

[ApiController]
[Route("api/properties")]
public class PropertiesApiController : ControllerBase
{
    private readonly PropertyService _propertyService;
    private readonly InMemoryDataStore _store;

    public PropertiesApiController(PropertyService propertyService, InMemoryDataStore store)
    {
        _propertyService = propertyService;
        _store = store;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_propertyService.GetAllProperties());
    }

    [HttpPost("search")]
    public IActionResult Search([FromBody] PropertySearchRequest request)
    {
        DateTime? startDate = null;
        DateTime? endDate = null;

        if (!string.IsNullOrWhiteSpace(request.StartDate) && DateTime.TryParse(request.StartDate, out var parsedStart))
        {
            startDate = parsedStart.Date;
        }

        if (!string.IsNullOrWhiteSpace(request.EndDate) && DateTime.TryParse(request.EndDate, out var parsedEnd))
        {
            endDate = parsedEnd.Date;
        }

        var results = _propertyService.SearchProperties(request.City, startDate, endDate);
        return Ok(results);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var property = _propertyService.GetById(id);
        if (property == null)
        {
            return NotFound(new { message = "Property not found." });
        }

        return Ok(property);
    }

    [Authorize(Roles = "OWNER")]
    [HttpPost]
    public IActionResult Create([FromBody] PropertyCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid property request." });
        }

        var ownerId = GetUserId();
        var ownerName = GetUserName();
        if (ownerId == null || ownerName == null)
        {
            return Unauthorized();
        }

        var property = _propertyService.Create(request, ownerId.Value, ownerName);
        return Ok(new { id = property.Id });
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] PropertyUpdateRequest request)
    {
        var ownerId = GetUserId();
        if (ownerId == null)
        {
            return Unauthorized();
        }

        var updated = _propertyService.Update(id, request, ownerId.Value);
        if (!updated)
        {
            return Forbid();
        }

        return NoContent();
    }

    [Authorize(Roles = "OWNER")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var ownerId = GetUserId();
        if (ownerId == null)
        {
            return Unauthorized();
        }

        var deleted = _propertyService.Delete(id, ownerId.Value);
        if (!deleted)
        {
            return Forbid();
        }

        return NoContent();
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : null;
    }

    private string? GetUserName()
    {
        return User.FindFirstValue("name") ?? User.Identity?.Name;
    }
}
