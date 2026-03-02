using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Contexts;
using TechExpress.Service.Enums;
using TechExpress.Service.Utils;

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
        public async Task<(Order order, List<Installment> installments)> HandleGuestCheckoutAsync(
            List<(Guid ProductId, int Quantity)> items,
            DeliveryType deliveryType,
            string? receiverEmail,
            string receiverFullName,
            string? shippingAddress,
            string trackingPhone, // Bắt buộc nhập từ Request
            PaidType paidType,
            string? receiverIdentityCard,
            int? installmentDurationMonth,
            string? notes)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // KIỂM TRA TRACKING PHONE CHO GUEST: Không được để trống
                if (string.IsNullOrWhiteSpace(trackingPhone))
                    throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var orderId = Guid.NewGuid();
                    var orderItems = new List<OrderItem>();
                    decimal subTotal = 0;

                    foreach (var item in items)
                    {
                        var product = await _unitOfWork.ProductRepository.FindByIdWithNoTrackingAsync(item.ProductId)
                            ?? throw new NotFoundException($"Sản phẩm không tồn tại.");

                        if (product.Status != ProductStatus.Available)
                            throw new BadRequestException($"Sản phẩm '{product.Name}' hiện không khả dụng.");

                        // --- CÁCH 2: ATOMIC UPDATE (THREAD-SAFE) ---
                        var affectedRows = await _unitOfWork.ProductRepository.DecrementStockAtomicAsync(item.ProductId, item.Quantity);
                        if (affectedRows == 0)
                            throw new BadRequestException($"Sản phẩm '{product.Name}' vừa hết hàng hoặc không đủ tồn kho.");

                        subTotal += product.Price * item.Quantity;

                        // FIX: Chỉ gán ProductId, không gán Product object để tránh lỗi PK Violation
                        orderItems.Add(new OrderItem { OrderId = orderId, ProductId = product.Id, Quantity = item.Quantity, UnitPrice = product.Price });
                    }

                    // Ràng buộc chung (Địa chỉ, Trả góp...)
                    ValidateOrderRequirements(deliveryType, shippingAddress, null, paidType, receiverIdentityCard, installmentDurationMonth);

                    var order = CreateOrderObject(orderId, null, deliveryType, subTotal, receiverEmail, receiverFullName,
                                                shippingAddress, trackingPhone, paidType, receiverIdentityCard,
                                                installmentDurationMonth, notes, orderItems);

                    await _unitOfWork.OrderRepository.AddOrderAsync(order);

                    var installmentList = new List<Installment>();
                    if (paidType == PaidType.Installment)
                    {
                        installmentList = await CreateInstallmentRecords(order, installmentDurationMonth!.Value);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // RELOAD: Lấy lại đơn hàng đầy đủ kèm Product Name từ DB để trả về response
                    var finalOrder = await GetOrderDetailsAsync(orderId);
                    return (finalOrder, installmentList);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // ============================== MEMBER CHECKOUT ===============================
        public async Task<(Order order, List<Installment> installments)> HandleMemberCheckoutAsync(
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
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
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

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // 4. Merge thông tin Profile (Logic C2) & Kiểm tra Tracking Phone
                    var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
                    var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
                    var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

                    string finalPhone;
                    // KIỂM TRA TRACKING PHONE:
                    if (string.IsNullOrWhiteSpace(user.Phone))
                    {
                        if (string.IsNullOrWhiteSpace(trackingPhone))
                            throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                        user.Phone = trackingPhone;
                        finalPhone = trackingPhone;
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(trackingPhone) && user.Phone != trackingPhone)
                        {
                            throw new BadRequestException("Số điện thoại không khớp với số điện thoại đã đăng ký tài khoản.");
                        }
                        finalPhone = user.Phone;
                    }

                    // 5. Kiểm tra ràng buộc
                    ValidateOrderRequirements(deliveryType, shippingAddress, user.Address, paidType, receiverIdentityCard, installmentDurationMonth);
                    if (string.IsNullOrWhiteSpace(finalFullName)) throw new BadRequestException("Tên người nhận không được để trống.");

                    // 6. Xử lý tồn kho (Cách 2: Atomic Update)
                    var orderId = Guid.NewGuid();
                    var orderItems = new List<OrderItem>();
                    decimal subTotal = 0;

                    foreach (var cartItem in selectedItems)
                    {
                        var product = await _unitOfWork.ProductRepository.FindByIdWithNoTrackingAsync(cartItem.ProductId)
                            ?? throw new NotFoundException($"Sản phẩm không còn tồn tại.");

                        var affectedRows = await _unitOfWork.ProductRepository.DecrementStockAtomicAsync(cartItem.ProductId, cartItem.Quantity);
                        if (affectedRows == 0)
                            throw new BadRequestException($"Sản phẩm '{product.Name}' vừa hết hàng hoặc không đủ tồn kho.");

                        subTotal += product.Price * cartItem.Quantity;

                        // FIX: Xóa "Product = product" để tránh lỗi PK constraint. Chỉ dùng ProductId.
                        orderItems.Add(new OrderItem { OrderId = orderId, ProductId = product.Id, Quantity = cartItem.Quantity, UnitPrice = product.Price });
                    }

                    // 7. Tạo Order
                    var order = CreateOrderObject(orderId, userId, deliveryType, subTotal, finalEmail, finalFullName,
                                                finalAddress, finalPhone, paidType, receiverIdentityCard,
                                                installmentDurationMonth, notes, orderItems);

                    await _unitOfWork.OrderRepository.AddOrderAsync(order);

                    // 8. Tích hợp tạo danh sách Installment Records (Ví dụ: 6 tháng tạo 6 bản ghi)
                    var installmentList = new List<Installment>();
                    if (paidType == PaidType.Installment)
                    {
                        installmentList = await CreateInstallmentRecords(order, installmentDurationMonth!.Value);
                    }

                    // 9. CHỈ XÓA các món đã mua và chốt giao dịch
                    foreach (var item in selectedItems)
                        _unitOfWork.CartItemRepository.RemoveCartItem(item);

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // RELOAD: Lấy lại đơn hàng đầy đủ kèm Product Name từ DB để trả về response chuẩn
                    var finalOrder = await GetOrderDetailsAsync(orderId);
                    return (finalOrder, installmentList);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // ============================== HELPER METHODS ===============================

        private async Task<List<Installment>> CreateInstallmentRecords(Order order, int duration)
        {
            var installments = new List<Installment>();
            decimal monthlyAmount = Math.Round(order.TotalPrice / duration, 0);

            for (int i = 1; i <= duration; i++)
            {
                var installment = new Installment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Period = i,
                    Amount = monthlyAmount,
                    Status = InstallmentStatus.Pending,
                    DueDate = DateTimeOffset.Now.AddMonths(i)
                };

                await _unitOfWork.InstallmentRepository.AddAsync(installment);
                installments.Add(installment);
            }
            return installments;
        }

        private void ValidateOrderRequirements(DeliveryType deliveryType, string? inputAddress, string? profileAddress,
                                             PaidType paidType, string? idCard, int? duration)
        {
            if (deliveryType == DeliveryType.Shipping && string.IsNullOrWhiteSpace(inputAddress) && string.IsNullOrWhiteSpace(profileAddress))
                throw new BadRequestException("Địa chỉ giao hàng là bắt buộc cho hình thức Shipping.");

            if (paidType == PaidType.Installment)
            {
                if (string.IsNullOrWhiteSpace(idCard))
                    throw new BadRequestException("Số định danh (CCCD) là bắt buộc khi chọn trả góp.");

                var validDurations = new[] { 6, 9, 12 };
                if (!duration.HasValue || !validDurations.Contains(duration.Value))
                    throw new BadRequestException("Kỳ hạn trả góp không hợp lệ. Chỉ hỗ trợ 6, 9 hoặc 12 tháng.");
            }
            else
            {
                if (duration.HasValue) throw new BadRequestException("Không thể chọn kỳ hạn thanh toán cho phương thức trả thẳng.");
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
                InstallmentDurationMonth = (paidType == PaidType.Installment) ? duration : null,
                Notes = notes,
                Status = OrderStatus.Pending,
                OrderDate = DateTimeOffset.Now,
                Items = items
            };
        }

        // ============================== CUSTOMER ORDER HISTORY ===============================

        public async Task<Pagination<Order>> HandleGetMyOrdersAsync(
            int page,
            int pageSize,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            SortDirection sortDirection = SortDirection.Desc,
            CancellationToken ct = default)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();

            var (items, totalCount) = await _unitOfWork.OrderRepository.GetPagedByUserIdAsync(
                userId, page, pageSize, orderStatus, paymentStatus,
                sortDirection == SortDirection.Asc, ct);

            return new Pagination<Order>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ============================== LIST ORDERS ===============================
        public async Task<Pagination<Order>> HandleGetOrderListWithPaginationAsync(
            int page,
            int pageSize,
            OrderSortBy sortBy,
            SortDirection sortDirection,
            string? search,
            OrderStatus? status)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var isDescending = sortDirection == SortDirection.Desc;

            var (orders, totalCount) = sortBy switch
            {
                OrderSortBy.TotalPrice => await _unitOfWork.OrderRepository
                    .FindOrdersPagedSortByTotalPriceAsync(page, pageSize, isDescending, search, status),

                _ => await _unitOfWork.OrderRepository
                    .FindOrdersPagedSortByOrderDateAsync(page, pageSize, isDescending, search, status)
            };

            return new Pagination<Order>
            {
                Items = orders,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ============================== ORDER DETAIL ===============================
        public async Task<(Order Order, List<Installment> Installments, List<Payment> Payments)>
            HandleGetOrderDetailAsync(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdIncludeItemsWithProductAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            var installments = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
            var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);

            return (order, installments, payments);
        }
        
        public async Task<Order> GetOrderDetailsAsync(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderByIdAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng này.");

            return order;
        }
    }
}