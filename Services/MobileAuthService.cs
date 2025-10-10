/************************************************************************************************
* Filename:         MobileAuthService.cs
* Course:           SE4040 - Enterprise Application Development
* Assignment:       EV Station Management System - User Management
* Student:          Wajee S (IT22094186)
* Date:             10-Oct-2025
*
* Description:
* This file provides the concrete implementation of the IMobileAuthService interface. It handles
* the unified login logic for the mobile application, which allows both Station Operators (via email)
* and EV Owners (via NIC) to authenticate through a single endpoint.
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
    /// Implements the IMobileAuthService to handle unified mobile user authentication.
    /// </summary>
    public class MobileAuthService : IMobileAuthService
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        // Method: Constructor for the MobileAuthService.
        // Injects dependencies for the database context and application configuration.
        public MobileAuthService(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Method: Handles the unified login process for a mobile user.
        // It first attempts to authenticate the identifier as a StationOperator's email. If that fails,
        // it then attempts to authenticate it as an EVOwner's NIC.
        public async Task<MobileLoginResponseDto?> MobileLoginAsync(MobileLoginDto mobileLoginDto)
        {
            // First, attempt to find a system user (StationOperator) with the given identifier as an email.
            var user = await _context.Users
                .Find(u => u.Email == mobileLoginDto.Identifier)
                .FirstOrDefaultAsync();

            // If a system user is found, proceed with their authentication process.
            if (user != null)
            {
                // Ensure the user's account is active.
                if (user.Status != UserStatus.Active)
                {
                    throw new InvalidOperationException("User account is inactive. Please contact administrator.");
                }

                // Verify the provided password against the stored hash.
                if (BCrypt.Net.BCrypt.Verify(mobileLoginDto.Password, user.PasswordHash))
                {
                    // Update the last login time for the user.
                    await UpdateLastLoginAsync(user.Id);

                    // Generate a JWT for the authenticated session.
                    var token = GenerateJwtToken(user.Id, user.Email, user.Role);
                    var expiresAt = DateTime.UtcNow.AddHours(
                        double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                    );

                    // Return a response tailored for a StationOperator.
                    return new MobileLoginResponseDto
                    {
                        UserId = user.Id,
                        Token = token,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        NIC = string.Empty, // Station operators do not use an NIC.
                        Role = user.Role.ToString(),
                        UserType = "StationOperator", // Specify the user type for client-side logic.
                        ExpiresAt = expiresAt
                    };
                }
            }

            // If no system user was found, attempt to find an EV Owner with the identifier as an NIC.
            var evOwner = await _context.EVOwners
                .Find(e => e.NIC == mobileLoginDto.Identifier)
                .FirstOrDefaultAsync();

            // If an EV Owner is found, proceed with their authentication process.
            if (evOwner != null)
            {
                // Ensure the EV Owner's account is active.
                if (!evOwner.IsActive)
                {
                    throw new InvalidOperationException("EV Owner account is inactive. Please contact administrator.");
                }

                // Verify the provided password against the stored hash.
                if (BCrypt.Net.BCrypt.Verify(mobileLoginDto.Password, evOwner.PasswordHash))
                {
                    // Generate a JWT for the authenticated EV Owner.
                    var token = GenerateJwtToken(evOwner.Id!, evOwner.NIC, UserRole.StationOperator); // Note: Assigning StationOperator role to EV Owner.
                    var expiresAt = DateTime.UtcNow.AddHours(
                        double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                    );

                    // Return a response tailored for an EV Owner.
                    return new MobileLoginResponseDto
                    {
                        UserId = evOwner.Id!,
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

            // If neither a StationOperator nor an EVOwner can be authenticated, return null.
            return null;
        }

        // Method: Generates a JSON Web Token (JWT) for an authenticated user.
        // This method creates a token with claims for user ID, identifier (email/NIC), and role.
        private string GenerateJwtToken(string userId, string identifier, UserRole role)
        {
            // Retrieve JWT settings from app configuration.
            var jwtSettings = _configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
            var issuer = jwtSettings["Issuer"] ?? "EVStationBackend";
            var audience = jwtSettings["Audience"] ?? "EVStationFrontend";
            var expirationHours = double.Parse(jwtSettings["ExpirationHours"] ?? "24");

            // Create security key and signing credentials.
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

            // Serialize the token into a string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Method: A private helper to update the LastLogin and UpdatedAt timestamps for a system user.
        private async Task UpdateLastLoginAsync(string userId)
        {
            // Define a filter to find the user by their ID.
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            
            // Define the update operation to set the new timestamps.
            var update = Builders<User>.Update
                .Set(u => u.LastLogin, DateTime.UtcNow)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            // Execute the update operation in the database.
            await _context.Users.UpdateOneAsync(filter, update);
        }
    }
}

