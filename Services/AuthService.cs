using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Data;
using ProductCatalogApi.Models;
using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
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
    }
}