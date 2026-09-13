using ProductCatalogApi.DTOs;

namespace ProductCatalogApi.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterDto dto);
    }
}