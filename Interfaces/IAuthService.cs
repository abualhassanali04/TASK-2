using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    }
}