using System;

namespace TechExpress.Application.Dtos.Responses
{
    /// <summary>
    /// Response khi kiểm tra bảo hành sản phẩm.
    /// </summary>
    public class WarrantyCheckResponse
    {
        /// <summary>
        /// OrderItemId được kiểm tra.
        /// </summary>
        public long OrderItemId { get; set; }

        /// <summary>
        /// Tên sản phẩm.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// SKU sản phẩm.
        /// </summary>
        public string ProductSku { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm bắt đầu bảo hành (ngày nhận hàng hoặc ngày đặt hàng).
        /// </summary>
        public DateTimeOffset WarrantyStartDate { get; set; }

        /// <summary>
        /// Thời lượng bảo hành (số tháng).
        /// </summary>
        public int WarrantyMonths { get; set; }

        /// <summary>
        /// Thời điểm hết hạn bảo hành.
        /// </summary>
        public DateTimeOffset WarrantyExpiredAt { get; set; }

        /// <summary>
        /// Thời điểm kiểm tra.
        /// </summary>
        public DateTimeOffset CheckedAt { get; set; }

        /// <summary>
        /// Còn bảo hành hay không tại thời điểm kiểm tra.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Số ngày còn lại (nếu còn bảo hành) hoặc số ngày đã quá hạn (nếu hết bảo hành).
        /// Giá trị dương = còn bảo hành, giá trị âm = đã quá hạn.
        /// </summary>
        public int RemainingDays { get; set; }

        /// <summary>
        /// Thông báo kết quả kiểm tra.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// TicketId nếu có message được gửi vào ticket.
        /// </summary>
        public Guid? TicketId { get; set; }

        /// <summary>
        /// MessageId nếu có message được tạo.
        /// </summary>
        public long? MessageId { get; set; }
    }
}
