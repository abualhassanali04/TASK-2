namespace ProductCatalogApi.Services
{
    public enum ServiceErrorType
    {
        NotFound,
        ValidationError,
        Conflict,
        Unauthorized
    }

    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ServiceErrorType ErrorType { get; private set; }

        private ServiceResult() { }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T> { Success = true, Data = data };
        }

        public static ServiceResult<T> Fail(string errorMessage, ServiceErrorType errorType)
        {
            return new ServiceResult<T> { Success = false, ErrorMessage = errorMessage, ErrorType = errorType };
        }
    }
}