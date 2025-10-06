using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using EVOwnerManagement.API.Data;
using EVOwnerManagement.API.DTOs;
using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users
                .Find(u => u.Email == loginDto.Email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            // Check if user is active
            if (user.Status != UserStatus.Active)
            {
                throw new InvalidOperationException("User account is inactive. Please contact administrator.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login time
            await UpdateLastLoginAsync(user.Id);

            // Generate JWT token
            var token = GenerateJwtToken(user.Id, user.Email, user.Role);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
            );

            return new LoginResponseDto
            {
                Token = token,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = expiresAt
            };
        }

        public string GenerateJwtToken(string userId, string identifier, UserRole role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = jwtSettings["Issuer"] ?? "EVStationBackend";
            var audience = jwtSettings["Audience"] ?? "EVStationFrontend";
            var expirationHours = double.Parse(jwtSettings["ExpirationHours"] ?? "24");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, identifier),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task UpdateLastLoginAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.LastLogin, DateTime.UtcNow)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            await _context.Users.UpdateOneAsync(filter, update);
        }
    }
}

