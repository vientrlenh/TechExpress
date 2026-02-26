using System.ComponentModel.DataAnnotations;

namespace TechExpress.Application.Dtos.Requests
{
    public class CreateReviewRequest
    {
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống.")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        public List<string>? MediaUrls { get; set; }
    }
}
