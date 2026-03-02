namespace TechExpress.Application.Common
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }

        public T? Value { get; set; }

        public static ApiResponse<T> OkResponse(T value)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status200OK,
                Value = value
            };
        }

        public static ApiResponse<T> CreatedResponse(T value)
        {
            return new ApiResponse<T>
            {
                StatusCode = StatusCodes.Status201Created,
                Value = value
            };
        }


    }


    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
