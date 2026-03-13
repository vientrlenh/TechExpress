using System;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Models;
using TechExpress.Service.Contexts;

namespace TechExpress.Service.Services
{
    public class WarrantySupportService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserContext _userContext;

        public WarrantySupportService(UnitOfWork unitOfWork, UserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        /// <summary>
        /// Kiểm tra bảo hành theo TicketId.
        /// </summary>
        public async Task<WarrantyCheckResult> CheckWarrantyByTicketIdAsync(
            Guid ticketId,
            DateTimeOffset? checkDate,
            CancellationToken ct = default)
        {
            // 1) Lấy ticket và order item
            var ticket = await _unitOfWork.TicketRepository.FindByIdAsync(ticketId)
                ?? throw new NotFoundException("Không tìm thấy ticket.");

            if (!ticket.OrderItemId.HasValue)
                throw new BadRequestException("Ticket này không liên kết với order item nào.");

            var orderItemId = ticket.OrderItemId.Value;

            // 2) Kiểm tra bảo hành
            var result = await CheckWarrantyByOrderItemIdInternalAsync(
                orderItemId,
                checkDate ?? DateTimeOffset.Now,
                ct);

            // 3) Gửi message vào ticket
            var message = CreateWarrantyMessage(result);
            var ticketMessage = new TicketMessage
            {
                TicketId = ticketId,
                UserId = _userContext.GetCurrentAuthenticatedUserId(),
                Content = message,
                IsStaffMessage = true
            };

            await _unitOfWork.TicketMessageRepository.AddAsync(ticketMessage);
            await _unitOfWork.SaveChangesAsync();

            result.TicketId = ticketId;
            result.MessageId = ticketMessage.Id;

            return result;
        }

        /// <summary>
        /// Kiểm tra bảo hành theo OrderItemId.
        /// </summary>
        public async Task<WarrantyCheckResult> CheckWarrantyByOrderItemIdAsync(
            long orderItemId,
            Guid? ticketId,
            DateTimeOffset? checkDate,
            CancellationToken ct = default)
        {
            // 1) Kiểm tra bảo hành
            var result = await CheckWarrantyByOrderItemIdInternalAsync(
                orderItemId,
                checkDate ?? DateTimeOffset.Now,
                ct);

            // 2) Nếu có ticketId thì gửi message vào ticket
            if (ticketId.HasValue)
            {
                var ticket = await _unitOfWork.TicketRepository.FindByIdWithTrackingAsync(ticketId.Value);
                if (ticket != null)
                {
                    var message = CreateWarrantyMessage(result);
                    var ticketMessage = new TicketMessage
                    {
                        TicketId = ticketId.Value,
                        UserId = _userContext.GetCurrentAuthenticatedUserId(),
                        Content = message,
                        IsStaffMessage = true
                    };

                    await _unitOfWork.TicketMessageRepository.AddAsync(ticketMessage);
                    await _unitOfWork.SaveChangesAsync();

                    result.TicketId = ticketId;
                    result.MessageId = ticketMessage.Id;
                }
            }

            return result;
        }

        /// <summary>
        /// Logic kiểm tra bảo hành nội bộ.
        /// </summary>
        private async Task<WarrantyCheckResult> CheckWarrantyByOrderItemIdInternalAsync(
            long orderItemId,
            DateTimeOffset checkDate,
            CancellationToken ct)
        {
            // 1) Lấy order item với order và product
            var orderItem = await _unitOfWork.OrderItemRepository.FindByIdAsync(orderItemId)
                ?? throw new NotFoundException("Không tìm thấy order item.");

            var order = orderItem.Order
                ?? throw new NotFoundException("Không tìm thấy đơn hàng liên quan.");

            var product = orderItem.Product
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            // 2) Xác định thời điểm bắt đầu bảo hành
            // Ưu tiên ReceivedAt, nếu không có thì dùng OrderDate
            var warrantyStartDate = order.ReceivedAt ?? order.OrderDate;

            // 3) Lấy thời lượng bảo hành từ snapshot
            var warrantyMonths = orderItem.WarrantyMonthSnapshot;

            // 4) Tính thời điểm hết hạn bảo hành
            var warrantyExpiredAt = warrantyStartDate.AddMonths(warrantyMonths);

            // 5) Kiểm tra còn bảo hành hay không tại thời điểm checkDate
            var isValid = checkDate < warrantyExpiredAt;

            // 6) Tính số ngày còn lại (hoặc đã quá hạn)
            var remainingDays = (int)(warrantyExpiredAt - checkDate).TotalDays;

            // 7) Tạo message
            string message;
            if (warrantyMonths == 0)
            {
                message = $"Sản phẩm '{product.Name}' (SKU: {product.Sku}) không có bảo hành.";
            }
            else if (isValid)
            {
                message = $"Sản phẩm '{product.Name}' (SKU: {product.Sku}) còn bảo hành. " +
                         $"Hết hạn vào: {warrantyExpiredAt:dd/MM/yyyy HH:mm}. " +
                         $"Còn lại: {remainingDays} ngày.";
            }
            else
            {
                message = $"Sản phẩm '{product.Name}' (SKU: {product.Sku}) đã hết bảo hành. " +
                         $"Hết hạn vào: {warrantyExpiredAt:dd/MM/yyyy HH:mm}. " +
                         $"Đã quá hạn: {Math.Abs(remainingDays)} ngày.";
            }

            return new WarrantyCheckResult
            {
                OrderItemId = orderItemId,
                ProductName = product.Name,
                ProductSku = product.Sku,
                WarrantyStartDate = warrantyStartDate,
                WarrantyMonths = warrantyMonths,
                WarrantyExpiredAt = warrantyExpiredAt,
                CheckedAt = checkDate,
                IsValid = isValid,
                RemainingDays = remainingDays,
                Message = message
            };
        }

        /// <summary>
        /// Tạo message cho ticket từ kết quả kiểm tra.
        /// </summary>
        private string CreateWarrantyMessage(WarrantyCheckResult result)
        {
            return $"Kiểm tra bảo hành:\n\n" +
                   $"Sản phẩm: {result.ProductName} (SKU: {result.ProductSku})\n" +
                   $"Bắt đầu bảo hành: {result.WarrantyStartDate:dd/MM/yyyy HH:mm}\n" +
                   $"Thời lượng: {result.WarrantyMonths} tháng\n" +
                   $"Hết hạn: {result.WarrantyExpiredAt:dd/MM/yyyy HH:mm}\n" +
                   $"Kiểm tra lúc: {result.CheckedAt:dd/MM/yyyy HH:mm}\n\n" +
                   $"Kết quả: {(result.IsValid ? "Còn bảo hành" : "Đã hết bảo hành")}\n" +
                   $"{(result.IsValid ? $"Còn lại: {result.RemainingDays} ngày" : $"Đã quá hạn: {Math.Abs(result.RemainingDays)} ngày")}";
        }
    }

    /// <summary>
    /// Kết quả kiểm tra bảo hành (internal).
    /// </summary>
    public class WarrantyCheckResult
    {
        public long OrderItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public DateTimeOffset WarrantyStartDate { get; set; }
        public int WarrantyMonths { get; set; }
        public DateTimeOffset WarrantyExpiredAt { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
        public bool IsValid { get; set; }
        public int RemainingDays { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? TicketId { get; set; }
        public long? MessageId { get; set; }
    }
}
