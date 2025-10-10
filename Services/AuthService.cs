/************************************************************************************************
* Filename:         AuthService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file provides the concrete implementation of the IAuthService interface. It contains
* the core logic for authenticating web users (Backoffice and StationOperator), verifying
* passwords against hashed values, and generating JSON Web Tokens (JWT) upon successful login.
************************************************************************************************/

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
    /// <summary>
    /// Implements the IAuthService interface to handle web user authentication.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        // Method: Constructor for the AuthService.
        // Injects dependencies for the database context and application configuration.
        public AuthService(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Method: Handles the login process for a web user.
        // It finds the user by email, verifies their account status and password,
        // updates their last login time, and returns a DTO with a valid JWT.
        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by email in the database.
            var user = await _context.Users
                .Find(u => u.Email == loginDto.Email)
                .FirstOrDefaultAsync();

            // Return null if no user is found with the given email.
            if (user == null)
            {
                return null;
            }

            // Check if the user's account is currently active.
            if (user.Status != UserStatus.Active)
            {
                // Throw an exception if the user is inactive to provide a specific error message.
                throw new InvalidOperationException("User account is inactive. Please contact administrator.");
            }

            // Verify the provided password against the stored hash.
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Return null if the password does not match.
            }

            // If login is successful, update the user's last login timestamp.
            await UpdateLastLoginAsync(user.Id);

            // Generate a JWT for the authenticated session.
            var token = GenerateJwtToken(user.Id, user.Email, user.Role);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
            );

            // Create and return the response object with user data and the token.
            return new LoginResponseDto
            {
                UserId = user.Id,
                Token = token,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = expiresAt
            };
        }

        // Method: Generates a JSON Web Token (JWT) for an authenticated user.
        // It creates a set of claims based on user data and signs the token using a secret key
        // from the application's configuration.
        public string GenerateJwtToken(string userId, string identifier, UserRole role)
        {
            // Retrieve JWT settings from appsettings.json.
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = jwtSettings["Issuer"] ?? "EVStationBackend";
            var audience = jwtSettings["Audience"] ?? "EVStationFrontend";
            var expirationHours = double.Parse(jwtSettings["ExpirationHours"] ?? "24");

            // Create the security key and signing credentials.
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the claims to be included in the token.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, identifier),
                new Claim(ClaimTypes.Role, role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // Create the JWT security token.
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            // Serialize the token into a string format.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Method: A private helper to update the LastLogin and UpdatedAt timestamps for a user.
        // This is called after a successful login to keep user records current.
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
