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
    public class MobileAuthService : IMobileAuthService
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public MobileAuthService(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<MobileLoginResponseDto?> MobileLoginAsync(MobileLoginDto mobileLoginDto)
        {
            // Try to authenticate as StationOperator (User) first
            var user = await _context.Users
                .Find(u => u.Email == mobileLoginDto.Identifier)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                // Check if user is active
                if (user.Status != UserStatus.Active)
                {
                    throw new InvalidOperationException("User account is inactive. Please contact administrator.");
                }

                // Verify password
                if (BCrypt.Net.BCrypt.Verify(mobileLoginDto.Password, user.PasswordHash))
                {
                    // Generate JWT token
                    var token = GenerateJwtToken(user.Id, user.Email, user.Role);
                    var expiresAt = DateTime.UtcNow.AddHours(
                        double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                    );

                    return new MobileLoginResponseDto
                    {
                        Token = token,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        NIC = string.Empty, // Station operators don't have NIC
                        Role = user.Role.ToString(),
                        UserType = "StationOperator",
                        ExpiresAt = expiresAt
                    };
                }
            }

            // TODO: EV Owner authentication - to be implemented by another teammate
            // EV Owner password authentication is handled by a different team member
            // This section will be uncommented once the EVOwner model includes password authentication
            
            /*
            // Try to authenticate as EVOwner
            var evOwner = await _context.EVOwners
                .Find(e => e.NIC == mobileLoginDto.Identifier)
                .FirstOrDefaultAsync();

            if (evOwner != null)
            {
                // Check if EV owner is active
                if (!evOwner.IsActive)
                {
                    throw new InvalidOperationException("EV Owner account is inactive. Please contact administrator.");
                }

                // Verify password
                if (BCrypt.Net.BCrypt.Verify(mobileLoginDto.Password, evOwner.PasswordHash))
                {
                    // Generate JWT token for EV owner
                    var token = GenerateJwtToken(evOwner.Id!, evOwner.NIC, UserRole.StationOperator); // Use StationOperator role for EV owners
                    var expiresAt = DateTime.UtcNow.AddHours(
                        double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                    );

                    return new MobileLoginResponseDto
                    {
                        Token = token,
                        FirstName = evOwner.FirstName,
                        LastName = evOwner.LastName,
                        Email = evOwner.Email,
                        NIC = evOwner.NIC,
                        Role = "EVOwner", // Custom role for EV owners
                        UserType = "EVOwner",
                        ExpiresAt = expiresAt
                    };
                }
            }
            */

            return null; // Authentication failed
        }

        private string GenerateJwtToken(string userId, string identifier, UserRole role)
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
    }
}
