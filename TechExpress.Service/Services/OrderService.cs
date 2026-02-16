// TechExpress.Service/Services/OrderService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Contexts;

namespace TechExpress.Service.Services
{
    public class OrderService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserContext _userContext;

        public OrderService(UnitOfWork unitOfWork, UserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Order> HandleGuestCheckoutAsync(
            List<(Guid ProductId, int Quantity)> items,
            DeliveryType deliveryType,
            string? receiverEmail,
            string receiverFullName,
            string? shippingAddress,
            string trackingPhone,
            PaidType paidType,
            string? receiverIdentityCard,
            int? installmentDurationMonth,
            string? notes)
        {
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>(); //
            decimal subTotal = 0;

            foreach (var item in items)
            {
                var product = await _unitOfWork.ProductRepository.FindByIdWithTrackingAsync(item.ProductId)
                    ?? throw new NotFoundException($"Sản phẩm {item.ProductId} không tồn tại."); //

                if (product.Status != ProductStatus.Available || product.Stock < item.Quantity)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho (Hiện có: {product.Stock}).");

                // Trừ Stock (Stock chính là Quantity trong kho)
                product.Stock -= item.Quantity;
                product.UpdatedAt = DateTimeOffset.Now;

                subTotal += product.Price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            // Ràng buộc địa chỉ giao hàng
            if (deliveryType == DeliveryType.Shipping && string.IsNullOrWhiteSpace(shippingAddress))
                throw new BadRequestException("Địa chỉ giao hàng là bắt buộc.");

            // Ràng buộc trả góp
            if (paidType == PaidType.Installment && (string.IsNullOrWhiteSpace(receiverIdentityCard) || !installmentDurationMonth.HasValue))
                throw new BadRequestException("Thông tin trả góp không đầy đủ.");

            var order = new Order //
            {
                Id = orderId,
                UserId = null,
                DeliveryType = deliveryType,
                SubTotal = subTotal,
                ShippingCost = (deliveryType == DeliveryType.Shipping ? 30000 : 0),
                Tax = subTotal * 0.1m,
                TotalPrice = subTotal + (deliveryType == DeliveryType.Shipping ? 30000 : 0) + (subTotal * 0.1m),
                ReceiverEmail = receiverEmail,
                ReceiverFullName = receiverFullName,
                ShippingAddress = shippingAddress,
                TrackingPhone = trackingPhone,
                Notes = notes,
                PaidType = paidType,
                ReceiverIdentityCard = receiverIdentityCard,
                InstallmentDurationMonth = installmentDurationMonth,
                Status = OrderStatus.Pending,
                OrderDate = DateTimeOffset.Now,
                Items = orderItems
            };

            await _unitOfWork.OrderRepository.AddOrderAsync(order);
            await _unitOfWork.SaveChangesAsync(); // Lưu mọi thay đổi trong 1 Transaction ngầm định

            return order;
        }
        // ============================== Memeber buy product===============================
        public async Task<Order> HandleMemberCheckoutAsync(
            Guid userId,
            List<Guid> selectedCartItemIds, // Nhận danh sách ID item được chọn
            DeliveryType deliveryType,
            string? receiverEmail,
            string? receiverFullName,
            string? shippingAddress,
            string? trackingPhone,
            PaidType paidType,
            string? receiverIdentityCard,
            int? installmentDurationMonth,
            string? notes)
        {
            // 1. Bảo mật: Kiểm tra xem người đang đăng nhập có phải là người sở hữu ID này không
            var authenticatedUserId = _userContext.GetCurrentAuthenticatedUserId();
            if (authenticatedUserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");
            }

            // 2. Lấy thông tin User và Giỏ hàng
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
                ?? throw new NotFoundException("Người dùng không tồn tại.");

            var cart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsWithTrackingAsync(userId)
                ?? throw new BadRequestException("Giỏ hàng của bạn đang trống.");

            if (cart.Items == null || !cart.Items.Any())
                throw new BadRequestException("Giỏ hàng của bạn đang trống.");

            // 3. Lọc ra những sản phẩm được người dùng tích chọn
            var selectedItems = cart.Items
                .Where(ci => selectedCartItemIds.Contains(ci.Id))
                .ToList();

            if (!selectedItems.Any())
            {
                throw new BadRequestException("Vui lòng chọn ít nhất một sản phẩm hợp lệ trong giỏ hàng để thanh toán.");
            }

            // 4. Tự động điền (Pre-fill) thông tin từ Profile
            var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
            var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
            var finalPhone = !string.IsNullOrWhiteSpace(trackingPhone) ? trackingPhone : user.Phone;

            // Logic C2: Kiểm tra Address gắt gao cho Shipping
            if (deliveryType == DeliveryType.Shipping && string.IsNullOrWhiteSpace(shippingAddress) && string.IsNullOrWhiteSpace(user.Address))
            {
                throw new BadRequestException("Địa chỉ giao hàng là bắt buộc cho hình thức Shipping.");
            }
            var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

            // Kiểm tra ràng buộc cơ bản sau merge
            if (string.IsNullOrWhiteSpace(finalFullName)) throw new BadRequestException("Tên người nhận không được để trống.");
            if (string.IsNullOrWhiteSpace(finalPhone)) throw new BadRequestException("Số điện thoại không được để trống.");

            // 5. Xử lý Stock và tạo OrderItems cho các món đã chọn
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var cartItem in selectedItems)
            {
                var product = await _unitOfWork.ProductRepository.FindByIdWithTrackingAsync(cartItem.ProductId)
                    ?? throw new NotFoundException($"Sản phẩm với ID {cartItem.ProductId} không còn tồn tại.");

                if (product.Status != ProductStatus.Available || product.Stock < cartItem.Quantity)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");

                // Trừ Stock trực tiếp
                product.Stock -= cartItem.Quantity;
                product.UpdatedAt = DateTimeOffset.Now;

                subTotal += product.Price * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    OrderId = orderId,
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price
                });
            }

            // 6. Khởi tạo đối tượng Order
            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                DeliveryType = deliveryType,
                SubTotal = subTotal,
                ShippingCost = (deliveryType == DeliveryType.Shipping ? 30000 : 0),
                Tax = subTotal * 0.1m,
                TotalPrice = subTotal + (deliveryType == DeliveryType.Shipping ? 30000 : 0) + (subTotal * 0.1m),
                ReceiverEmail = finalEmail,
                ReceiverFullName = finalFullName,
                ShippingAddress = finalAddress,
                TrackingPhone = finalPhone!,
                PaidType = paidType,
                ReceiverIdentityCard = receiverIdentityCard,
                InstallmentDurationMonth = installmentDurationMonth,
                Notes = notes,
                Status = OrderStatus.Pending,
                OrderDate = DateTimeOffset.Now,
                Items = orderItems
            };

            // 7. Lưu đơn hàng và CHỈ XÓA các Item đã chọn mua
            await _unitOfWork.OrderRepository.AddOrderAsync(order);

            foreach (var item in selectedItems)
            {
                // Bạn cần đảm bảo Repository có hàm xóa lẻ item
                _unitOfWork.CartItemRepository.RemoveCartItem(item);
            }

            await _unitOfWork.SaveChangesAsync(); // Kết thúc giao dịch ngầm định

            return order;
        }
    }
}