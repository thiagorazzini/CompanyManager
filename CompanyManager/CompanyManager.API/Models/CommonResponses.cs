namespace CompanyManager.API.Models
{
    /// <summary>
    /// Base response for all API responses
    /// </summary>
    public abstract class BaseResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Standard error response
    /// </summary>
    public class ErrorResponse : BaseResponse
    {
        public ErrorResponse(string message)
        {
            Message = message;
        }
    }

    /// <summary>
    /// Validation error response with detailed errors
    /// </summary>
    public class ValidationErrorResponse : ErrorResponse
    {
        public List<string> Errors { get; set; } = new();

        public ValidationErrorResponse(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Pagination information for list responses
    /// </summary>
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Standard success response
    /// </summary>
    public class SuccessResponse : BaseResponse
    {
        public SuccessResponse(string message)
        {
            Message = message;
        }
    }
}
