using System;

namespace TechExpress.Application.Dtos.Requests
{
    /// <summary>
    /// Request để kiểm tra bảo hành theo TicketId.
    /// </summary>
    public class CheckWarrantyByTicketIdRequest
    {
        /// <summary>
        /// Ngày kiểm tra (optional). Nếu null thì dùng DateTimeOffset.Now.
        /// </summary>
        public DateTimeOffset? CheckDate { get; set; }
    }
}
