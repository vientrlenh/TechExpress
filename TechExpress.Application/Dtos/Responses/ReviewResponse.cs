namespace TechExpress.Application.Dtos.Responses
{
    public class ReviewMediaResponse
    {
        public long Id { get; set; }
        public string? MediaUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class ReviewResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int Rating { get; set; }
        public ICollection<ReviewMediaResponse> Medias { get; set; } = [];
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
