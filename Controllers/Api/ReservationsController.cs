using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pruebaAsp.Models;
using pruebaAsp.Services;
using System.Security.Claims;

namespace pruebaAsp.Controllers.Api;

[ApiController]
[Route("api/reservations")]
public class ReservationsApiController : ControllerBase
{
    private readonly InMemoryDataStore _store;

    public ReservationsApiController(InMemoryDataStore store)
    {
        _store = store;
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create([FromBody] ReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid reservation request." });
        }

        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var property = _store.Properties.FirstOrDefault(p => p.Id == request.PropertyId);
        if (property == null)
        {
            return NotFound(new { message = "Property not found." });
        }

        if (!DateTime.TryParse(request.StartDate, out var startDate) || !DateTime.TryParse(request.EndDate, out var endDate))
        {
            return BadRequest(new { message = "Invalid date format." });
        }

        if (startDate.Date >= endDate.Date)
        {
            return BadRequest(new { message = "startDate must be before endDate." });
        }

        if (_store.Reservations.Any(r => r.PropertyId == request.PropertyId && DatesOverlap(startDate.Date, endDate.Date, r.StartDate, r.EndDate)))
        {
            return BadRequest(new { message = "Dates overlap with existing reservation" });
        }

        var totalDays = (endDate.Date - startDate.Date).Days;
        var totalPrice = totalDays * property.PricePerNight;

        var reservation = new Reservation
        {
            Id = _store.GetNextReservationId(),
            UserId = userId.Value,
            PropertyId = request.PropertyId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalPrice = totalPrice
        };

        _store.Reservations.Add(reservation);
        return Ok(new { id = reservation.Id });
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_store.Reservations);
    }

    [Authorize]
    [HttpGet("byuser")]
    public IActionResult GetByUser()
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reservations = _store.Reservations.Where(r => r.UserId == userId.Value);
        return Ok(reservations);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult Cancel(int id)
    {
        var userId = GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reservation = _store.Reservations.FirstOrDefault(r => r.Id == id && r.UserId == userId.Value);
        if (reservation == null)
        {
            return NotFound(new { message = "Reservation not found." });
        }

        _store.Reservations.Remove(reservation);
        return NoContent();
    }

    [Authorize(Roles = "OWNER")]
    [HttpGet("metrics")]
    public IActionResult GetDashboardMetrics()
    {
        var ownerId = GetUserId();
        if (ownerId == null)
        {
            return Unauthorized();
        }

        var ownerProperties = _store.Properties.Where(p => p.OwnerId == ownerId.Value).ToList();
        var totalProperties = ownerProperties.Count;
        var totalReservations = _store.Reservations.Count(r => ownerProperties.Any(p => p.Id == r.PropertyId));
        var totalEarnings = ownerProperties.Sum(p => _store.Reservations.Where(r => r.PropertyId == p.Id).Sum(r => r.TotalPrice));
        var totalPossibleNights = ownerProperties.Sum(p => p.PricePerNight > 0 ? 1 : 0); // placeholder, not used for occupancy

        var propertyMetrics = ownerProperties.Select(p =>
        {
            var reservations = _store.Reservations.Where(r => r.PropertyId == p.Id).ToList();
            var totalNights = reservations.Sum(r => (r.EndDate.Date - r.StartDate.Date).Days);
            var occupancyRate = totalNights == 0 ? 0.0 : Math.Min(100.0, Math.Round((totalNights / 30.0) * 100, 2));

            return new PropertyMetrics
            {
                Id = p.Id,
                Title = p.Title,
                ReservationCount = reservations.Count,
                Earnings = reservations.Sum(r => r.TotalPrice),
                OccupancyRate = occupancyRate
            };
        }).ToList();

        var metrics = new DashboardMetrics
        {
            TotalProperties = totalProperties,
            TotalEarnings = totalEarnings,
            TotalReservations = totalReservations,
            OccupancyRate = propertyMetrics.Count == 0 ? 0.0 : Math.Round(propertyMetrics.Average(pm => pm.OccupancyRate), 2),
            Properties = propertyMetrics
        };

        return Ok(metrics);
    }

    private static bool DatesOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
    {
        return startA < endB && startB < endA;
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : null;
    }
}
