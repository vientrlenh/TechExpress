namespace TechExpress.Application.DTOs.Requests
{
    public class PromotionFilterRequest
    {
        public string? Search { get; set; } // Tìm theo Name hoặc Code
        public bool? IsActive { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public bool IsDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20; // Default là 20, nhưng có thể truyền từ Client
    }
}
