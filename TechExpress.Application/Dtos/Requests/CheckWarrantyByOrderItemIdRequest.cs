using System;

namespace TechExpress.Application.Dtos.Requests
{
    /// <summary>
    /// Request để kiểm tra bảo hành theo OrderItemId.
    /// </summary>
    public class CheckWarrantyByOrderItemIdRequest
    {
        /// <summary>
        /// TicketId (optional). Nếu có thì sẽ gửi message vào ticket này.
        /// </summary>
        public Guid? TicketId { get; set; }

        /// <summary>
        /// Ngày kiểm tra (optional). Nếu null thì dùng DateTimeOffset.Now.
        /// </summary>
        public DateTimeOffset? CheckDate { get; set; }
    }
}
