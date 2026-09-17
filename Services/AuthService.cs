using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProductCatalogApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto)
        {
            var usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (usernameExists)
            {
                _logger.LogWarning("Attempted to register with an existing username: {Username}", dto.Username);
                return ServiceResult<UserDto>.Fail("Username is already taken.", ServiceErrorType.ValidationError);
            }

            var user = new User
            {
                Username = dto.Username,
                Role = "User"
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered with id {Id}", user.Username, user.Id);

            var resultDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return ServiceResult<UserDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with unknown username: {Username}", dto.Username);
                return ServiceResult<AuthResponseDto>.Fail("Invalid username or password.", ServiceErrorType.ValidationError);
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Failed login attempt for username: {Username}", dto.Username);
                return ServiceResult<AuthResponseDto>.Fail("Invalid username or password.", ServiceErrorType.ValidationError);
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            return ServiceResult<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        public async Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return ServiceResult<AuthResponseDto>.Fail("Invalid or expired refresh token.", ServiceErrorType.ValidationError);
            }

            storedToken.IsRevoked = true;

            var newAccessToken = GenerateJwtToken(storedToken.User);
            var newRefreshToken = await GenerateRefreshTokenAsync(storedToken.UserId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Token refreshed for user {UserId}", storedToken.UserId);

            return ServiceResult<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }
    }
}