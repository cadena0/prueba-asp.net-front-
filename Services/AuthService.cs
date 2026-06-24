using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using pruebaAsp.Models;

namespace pruebaAsp.Services
{
    public class AuthService
    {
        private readonly InMemoryDataStore _store;
        private readonly JwtSettings _jwtSettings;
        private readonly string? _initialOwnerEmail;
        private readonly string? _initialAdminEmail;

        public AuthService(InMemoryDataStore store, IConfiguration configuration)
        {
            _store = store;
            _jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? string.Empty);
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException($"Jwt key is too short ({keyBytes.Length} bytes). Configure Jwt:Key with at least 32 bytes in appsettings.");
            }
            _initialOwnerEmail = configuration["InitialOwner:Email"]?.Trim();
            _initialAdminEmail = configuration["InitialAdmin:Email"]?.Trim();
        }

        public AuthResponse Register(RegisterRequest request)
        {
            request.FullName = request.FullName?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Password = request.Password?.Trim() ?? string.Empty;

            var existing = _store.Users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return new AuthResponse { Success = false, Token = string.Empty };
            }

            // Determine role server-side: ADMIN or OWNER only if email equals the configured initial admin/owner email and none exist yet
            var role = "GUEST";
            if (!string.IsNullOrWhiteSpace(_initialAdminEmail) &&
                request.Email.Equals(_initialAdminEmail, StringComparison.OrdinalIgnoreCase) &&
                !_store.Users.Any(u => u.Role == "ADMIN"))
            {
                role = "ADMIN";
            }
            else if (!string.IsNullOrWhiteSpace(_initialOwnerEmail) &&
                request.Email.Equals(_initialOwnerEmail, StringComparison.OrdinalIgnoreCase) &&
                !_store.Users.Any(u => u.Role == "OWNER"))
            {
                role = "OWNER";
            }

            var user = new User
            {
                Id = _store.GetNextUserId(),
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password,
                Role = role
            };

            _store.Users.Add(user);
            var token = CreateToken(user);
            return new AuthResponse { Success = true, Token = token };
        }

        public AuthResponse Login(LoginRequest request)
        {
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Password = request.Password?.Trim() ?? string.Empty;

            var user = _store.Users.FirstOrDefault(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase) && u.Password == request.Password);
            if (user == null)
            {
                return new AuthResponse { Success = false, Token = string.Empty };
            }

            var token = CreateToken(user);
            return new AuthResponse { Success = true, Token = token };
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("role", user.Role),
                new Claim("name", user.FullName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
