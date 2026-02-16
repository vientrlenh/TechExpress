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

        // ============================== GUEST CHECKOUT ===============================
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
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var item in items)
            {
                var product = await _unitOfWork.ProductRepository.FindByIdWithTrackingAsync(item.ProductId)
                    ?? throw new NotFoundException($"Sản phẩm {item.ProductId} không tồn tại.");

                if (product.Status != ProductStatus.Available || product.Stock < item.Quantity)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho (Hiện có: {product.Stock}).");

                // Trừ Stock & Update thời gian (Chống Race Condition)
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

            // Ràng buộc chung
            ValidateOrderRequirements(deliveryType, shippingAddress, null, paidType, receiverIdentityCard, installmentDurationMonth);

            var order = CreateOrderObject(orderId, null, deliveryType, subTotal, receiverEmail, receiverFullName,
                                        shippingAddress, trackingPhone, paidType, receiverIdentityCard,
                                        installmentDurationMonth, notes, orderItems);

            await _unitOfWork.OrderRepository.AddOrderAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order;
        }

        // ============================== MEMBER CHECKOUT ===============================
        public async Task<Order> HandleMemberCheckoutAsync(
            Guid userId,
            List<Guid> selectedCartItemIds,
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
            // 1. Bảo mật: Chặn đứng ý định "thanh toán dùm" ID người khác
            var authenticatedUserId = _userContext.GetCurrentAuthenticatedUserId();
            if (authenticatedUserId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");

            // 2. Lấy thông tin User và Giỏ hàng
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
                ?? throw new NotFoundException("Người dùng không tồn tại.");

            var cart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsWithTrackingAsync(userId)
                ?? throw new BadRequestException("Giỏ hàng của bạn đang trống.");

            // 3. Lọc sản phẩm được chọn
            var selectedItems = cart.Items.Where(ci => selectedCartItemIds.Contains(ci.Id)).ToList();
            if (!selectedItems.Any())
                throw new BadRequestException("Vui lòng chọn ít nhất một sản phẩm hợp lệ để thanh toán.");

            // 4. Merge thông tin Profile (Logic C2)
            var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
            var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
            var finalPhone = !string.IsNullOrWhiteSpace(trackingPhone) ? trackingPhone : user.Phone;
            var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

            // 5. Kiểm tra ràng buộc
            ValidateOrderRequirements(deliveryType, shippingAddress, user.Address, paidType, receiverIdentityCard, installmentDurationMonth);
            if (string.IsNullOrWhiteSpace(finalFullName)) throw new BadRequestException("Tên người nhận không được để trống.");
            if (string.IsNullOrWhiteSpace(finalPhone)) throw new BadRequestException("Số điện thoại không được để trống.");

            // 6. Xử lý tồn kho
            var orderId = Guid.NewGuid();
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var cartItem in selectedItems)
            {
                var product = await _unitOfWork.ProductRepository.FindByIdWithTrackingAsync(cartItem.ProductId)
                    ?? throw new NotFoundException($"Sản phẩm với ID {cartItem.ProductId} không còn tồn tại.");

                if (product.Status != ProductStatus.Available || product.Stock < cartItem.Quantity)
                    throw new BadRequestException($"Sản phẩm '{product.Name}' không đủ tồn kho.");

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

            // 7. Tạo Order & Lưu
            var order = CreateOrderObject(orderId, userId, deliveryType, subTotal, finalEmail, finalFullName,
                                        finalAddress, finalPhone!, paidType, receiverIdentityCard,
                                        installmentDurationMonth, notes, orderItems);

            await _unitOfWork.OrderRepository.AddOrderAsync(order);

            // 8. CHỈ XÓA các món đã mua
            foreach (var item in selectedItems)
                _unitOfWork.CartItemRepository.RemoveCartItem(item);

            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        // ============================== HELPER METHODS ===============================

        private void ValidateOrderRequirements(DeliveryType deliveryType, string? inputAddress, string? profileAddress,
                                             PaidType paidType, string? idCard, int? duration)
        {
            // Kiểm tra địa chỉ ship
            if (deliveryType == DeliveryType.Shipping && string.IsNullOrWhiteSpace(inputAddress) && string.IsNullOrWhiteSpace(profileAddress))
                throw new BadRequestException("Địa chỉ giao hàng là bắt buộc cho hình thức Shipping.");

            // Kiểm tra trả góp (CCCD và kỳ hạn 6-9-12)
            if (paidType == PaidType.Installment)
            {
                if (string.IsNullOrWhiteSpace(idCard))
                    throw new BadRequestException("Thông tin số định danh (CCCD) là bắt buộc khi trả góp.");

                var validDurations = new[] { 6, 9, 12 };
                if (!duration.HasValue || !validDurations.Contains(duration.Value))
                    throw new BadRequestException("Kỳ hạn trả góp không hợp lệ. Chỉ hỗ trợ 6, 9 hoặc 12 tháng.");
            }
        }

        private Order CreateOrderObject(Guid id, Guid? userId, DeliveryType deliveryType, decimal subTotal,
                                      string? email, string name, string? address, string phone,
                                      PaidType paidType, string? idCard, int? duration, string? notes,
                                      List<OrderItem> items)
        {
            decimal shippingCost = (deliveryType == DeliveryType.Shipping ? 30000 : 0);
            decimal tax = subTotal * 0.1m;

            return new Order
            {
                Id = id,
                UserId = userId,
                DeliveryType = deliveryType,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Tax = tax,
                TotalPrice = subTotal + shippingCost + tax,
                ReceiverEmail = email,
                ReceiverFullName = name,
                ShippingAddress = address,
                TrackingPhone = phone,
                PaidType = paidType,
                ReceiverIdentityCard = idCard,
                InstallmentDurationMonth = duration,
                Notes = notes,
                Status = OrderStatus.Pending,
                OrderDate = DateTimeOffset.Now,
                Items = items
            };
        }
    }
}