using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Requests
{
    public class UpdateOrderStatusRequest
    {
        public required OrderStatus Status { get; set; }

        /// <summary>
        /// Dùng khi chuyển sang Shipping: ID nhân viên hệ thống tự vận chuyển.
        /// Nếu set thì chỉ nhân viên này mới có thể cập nhật Delivered.
        /// </summary>
        public Guid? DeliveredById { get; set; }

        /// <summary>
        /// Dùng khi chuyển sang Shipping: tên dịch vụ vận chuyển bên thứ 3 (VD: GHN, GHTK).
        /// Nếu set thì bất kỳ nhân viên nào cũng có thể cập nhật Delivered.
        /// </summary>
        public string? CourierService { get; set; }

        /// <summary>
        /// Mã vận đơn từ bên vận chuyển thứ 3.
        /// </summary>
        public string? CourierTrackingCode { get; set; }
    }
}
