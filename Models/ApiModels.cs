using System.ComponentModel.DataAnnotations;

namespace pruebaAsp.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = "super-secret-key-12345-super-secret-key-12345-super-secret-key-12345-super-secret-key-12345";
        public string Issuer { get; set; } = "pruebaAsp";
        public string Audience { get; set; } = "pruebaAsp";
    }

    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "GUEST";
    }

    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
    }

    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class PropertyCreateRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public decimal PricePerNight { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class PropertyUpdateRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public decimal? PricePerNight { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class PropertySearchRequest
    {
        public string? City { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }

    public class ReservationRequest
    {
        [Required]
        public int PropertyId { get; set; }
        [Required]
        public string StartDate { get; set; } = string.Empty;
        [Required]
        public string EndDate { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class DashboardMetrics
    {
        public int TotalProperties { get; set; }
        public decimal TotalEarnings { get; set; }
        public double OccupancyRate { get; set; }
        public int TotalReservations { get; set; }
        public List<PropertyMetrics> Properties { get; set; } = new();
    }

    public class PropertyMetrics
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ReservationCount { get; set; }
        public decimal Earnings { get; set; }
        public double OccupancyRate { get; set; }
    }
}
