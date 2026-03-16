namespace TechExpress.Repository.Enums;

public enum NotificationType
{
    OrderPlaced, // thông báo về trạng thái đơn hàng
    OrderShipped, // thông báo về trạng thái đơn hàng
    OrderDelievered, // thông báo về trạng thái đơn hàng
    OrderCancelled, // thông báo về trạng thái đơn hàng
    PaymentSuccess, // thông báo về thanh toán thành công hoặc thất bại
    PaymentFailed, // thông báo về thanh toán thành công hoặc thất bại
    PromotionAlert, // thông báo khuyến mãi 
    StockAlert, // sản phẩm hết hàng  
    TicketAlert, // thông báo về ticket hỗ trợ khách hàng 
    ReviewResponse, // phản hồi về đánh giá của khách hàng
    System // thông báo hệ thống chung (ví dụ: bảo trì, cập nhật chính sách, v.v.)
}
