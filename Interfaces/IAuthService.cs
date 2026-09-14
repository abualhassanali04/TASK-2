using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<string>> LoginAsync(LoginDto dto);
    }
}