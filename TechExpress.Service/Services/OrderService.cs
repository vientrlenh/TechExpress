using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Commands;
using TechExpress.Service.Contexts;
using TechExpress.Service.Enums;
using TechExpress.Service.Dtos;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class OrderService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserContext _userContext;
        private readonly PromotionService _promotionService;

        public OrderService(UnitOfWork unitOfWork, UserContext userContext, PromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _promotionService = promotionService;
        }

        // ============================== GUEST CHECKOUT ===============================
        // ĐÃ CẬP NHẬT: Thêm List<PromotionUsage> vào tuple trả về
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleGuestCheckoutAsync(
            List<(Guid ProductId, int Quantity)> items,
            List<string>? promotionCodes,
            List<Guid>? chosenFreeProductIds,
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
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(trackingPhone))
                    throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                // Sử dụng Hàm Helper Dùng Chung
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId: null,
                    rawItems: items,
                    promotionCodes, chosenFreeProductIds, deliveryType, receiverEmail, receiverFullName,
                    shippingAddress, trackingPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes
                );
            });
        }

        // ============================== MEMBER CHECKOUT ===============================
        // ĐÃ CẬP NHẬT: Thêm List<PromotionUsage> vào tuple trả về
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleMemberCheckoutAsync(
            Guid userId,
            List<Guid> selectedCartItemIds,
            List<string>? promotionCodes,
            List<Guid>? chosenFreeProductIds,
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
                var authenticatedUserId = _userContext.GetCurrentAuthenticatedUserId();

                if (authenticatedUserId != userId)
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");

                var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
                    ?? throw new NotFoundException("Người dùng không tồn tại.");

                var cart = await _unitOfWork.CartRepository
                    .FindCartByUserIdIncludeItemsWithTrackingAsync(userId)
                    ?? throw new BadRequestException("Giỏ hàng của bạn đang trống.");

                var selectedItems = cart.Items
                    .Where(ci => selectedCartItemIds.Contains(ci.Id))
                    .ToList();

                if (!selectedItems.Any())
                    throw new BadRequestException("Vui lòng chọn ít nhất một sản phẩm.");

                var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
                var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
                var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

                // --- Logic xử lý số điện thoại ---
                string finalPhone;
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
                        throw new BadRequestException("Số điện thoại không khớp.");

                    finalPhone = user.Phone;
                }

                // Chuyển CartItem thành dạng List raw
                var rawItems = selectedItems.Select(ci => (ci.ProductId, ci.Quantity)).ToList();

                // Sử dụng Hàm Helper Dùng Chung
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId, rawItems, promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes,
                    onBeforeSaveAsync: async () =>
                    {
                        foreach (var item in selectedItems)
                            _unitOfWork.CartItemRepository.RemoveCartItem(item);
                        await Task.CompletedTask;
                    }
                );
            });
        }

        // ============================== CUSTOM PC CHECKOUT ===============================
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleCustomPCCheckoutAsync(
            Guid userId,
            string? sessionId,
            Guid customPCId,
            List<string>? promotionCodes,
            List<Guid>? chosenFreeProductIds,
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
                var authenticatedUserId = _userContext.GetCurrentAuthenticatedUserId();

                if (authenticatedUserId != userId)
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");

                var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
                    ?? throw new NotFoundException("Người dùng không tồn tại.");

                var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId)
                    ?? throw new NotFoundException("Không tìm thấy cấu hình PC này.");

                // ================== CẬP NHẬT LOGIC CHECK QUYỀN (CHO PHÉP CLAIM PC) ==================
                bool isOwner = false;

                if (customPC.UserId.HasValue)
                {
                    // Trường hợp 1: Cấu hình PC này đã được tạo lúc User ĐÃ ĐĂNG NHẬP
                    isOwner = (customPC.UserId.Value == userId);
                }
                else
                {
                    // Trường hợp 2: Cấu hình PC này tạo lúc User CHƯA ĐĂNG NHẬP (Guest)
                    // -> Chỉ cần frontend truyền đúng SessionId lúc nãy lên là cho phép thanh toán luôn.
                    isOwner = !string.IsNullOrWhiteSpace(sessionId) && customPC.SessionId == sessionId;

                    // Tùy chọn: Có thể gán lại UserId để "xác nhận chính chủ" luôn trước khi save
                    if (isOwner) customPC.UserId = userId;
                }

                if (!isOwner)
                {
                    throw new ForbiddenException("Bạn không có quyền thanh toán cấu hình PC này.");
                }
                // ====================================================================================

                if (customPC.Items == null || !customPC.Items.Any())
                    throw new BadRequestException("Cấu hình PC này chưa có linh kiện nào.");

                var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
                var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
                var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

                // --- ĐÃ TỐI ƯU: Logic xử lý số điện thoại gọn gàng hơn ---
                if (string.IsNullOrWhiteSpace(user.Phone) && string.IsNullOrWhiteSpace(trackingPhone))
                    throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                if (!string.IsNullOrWhiteSpace(user.Phone) && !string.IsNullOrWhiteSpace(trackingPhone) && user.Phone != trackingPhone)
                    throw new BadRequestException("Số điện thoại không khớp với hồ sơ.");

                string finalPhone = string.IsNullOrWhiteSpace(user.Phone) ? trackingPhone! : user.Phone;
                user.Phone = finalPhone; // Cập nhật số điện thoại cho User nếu họ chưa có

                var rawItems = customPC.Items.Select(ci => (ci.ProductId, ci.Quantity)).ToList();

                // Sử dụng Hàm Helper Dùng Chung
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId, rawItems, promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes,
                    onBeforeSaveAsync: async () =>
                    {
                        // --- ĐÃ TỐI ƯU: Dọn dẹp Custom PC sau khi đã mua ---
                        // Tùy vào repository của bạn có hàm Remove hoặc Delete
                        // _unitOfWork.CustomPCRepository.Delete(customPC); 
                        await Task.CompletedTask;
                    }
                );
            });
        }

        //================================ CUSTOMPC CHECKOUT CHO STAFF TAO ORDER=============================
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleCustomPCStaffCheckoutAsync(
            Guid userId,
            string? sessionId,
            Guid customPCId,
            List<string>? promotionCodes,
            List<Guid>? chosenFreeProductIds,
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
                var authenticatedUserId = _userContext.GetCurrentAuthenticatedUserId();

                if (authenticatedUserId != userId)
                    throw new UnauthorizedAccessException("Bạn không có quyền thực hiện hành động này.");

                var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId)
                    ?? throw new NotFoundException("Người dùng không tồn tại.");

                var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId)
                    ?? throw new NotFoundException("Không tìm thấy cấu hình PC này.");

                // ================== CẬP NHẬT LOGIC CHECK QUYỀN (CHO PHÉP CLAIM PC) ==================
                bool isOwner = false;

                if (customPC.UserId.HasValue)
                {
                    isOwner = (customPC.UserId.Value == userId);
                }
                else
                {
                    isOwner = !string.IsNullOrWhiteSpace(sessionId) && customPC.SessionId == sessionId;
                    if (isOwner) customPC.UserId = userId;
                }

                if (!isOwner)
                {
                    throw new ForbiddenException("Bạn không có quyền thanh toán cấu hình PC này.");
                }
                // ====================================================================================

                if (customPC.Items == null || !customPC.Items.Any())
                    throw new BadRequestException("Cấu hình PC này chưa có linh kiện nào.");

                var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName) ? receiverFullName : $"{user.FirstName} {user.LastName}".Trim();
                var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user.Email;
                var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user.Address;

                if (string.IsNullOrWhiteSpace(user.Phone) && string.IsNullOrWhiteSpace(trackingPhone))
                    throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                if (!string.IsNullOrWhiteSpace(user.Phone) && !string.IsNullOrWhiteSpace(trackingPhone) && user.Phone != trackingPhone)
                    throw new BadRequestException("Số điện thoại không khớp với hồ sơ.");

                string finalPhone = string.IsNullOrWhiteSpace(user.Phone) ? trackingPhone! : user.Phone;
                user.Phone = finalPhone;

                var rawItems = customPC.Items.Select(ci => (ci.ProductId, ci.Quantity)).ToList();

                // Sử dụng Hàm Helper Dùng Chung
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId, rawItems, promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes,
                    onBeforeSaveAsync: async () =>
                    {
                        // _unitOfWork.CustomPCRepository.Delete(customPC); 
                        await Task.CompletedTask;
                    }
                );
            });
        }

        // ============================== CORE CHECKOUT LOGIC HELPER ===============================
        /// <summary>
        /// Hàm đóng gói xử lý Transaction chung cho tất cả các luồng Checkout (Guest, Member, CustomPC).
        /// </summary>
        private async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> ExecuteCoreCheckoutInTransactionAsync(
            Guid? userId,
            List<(Guid ProductId, int Quantity)> rawItems,
            List<string>? promotionCodes,
            List<Guid>? chosenFreeProductIds,
            DeliveryType deliveryType,
            string? receiverEmail,
            string receiverFullName,
            string? shippingAddress,
            string trackingPhone,
            PaidType paidType,
            string? receiverIdentityCard,
            int? installmentDurationMonth,
            string? notes,
            Func<Task>? onBeforeSaveAsync = null)
        {
            ValidateOrderRequirements(deliveryType, shippingAddress, null, paidType, receiverIdentityCard, installmentDurationMonth);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orderId = Guid.NewGuid();
                var orderItems = new List<OrderItem>();
                var checkoutCommands = new List<CheckoutItemCommand>();
                decimal subTotal = 0;

                // ===== GROUP ITEMS ĐỂ TRÁNH DUPLICATE PRODUCT =====
                var groupedItems = rawItems
                    .GroupBy(x => x.ProductId)
                    .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                    .ToList();

                var productIds = groupedItems.Select(i => i.ProductId).Distinct().ToList();

                // Tối ưu danh sách request trừ kho
                var requestProducts = groupedItems.Select(i => (i.ProductId, i.Quantity)).ToList();

                var products = await _unitOfWork.ProductRepository.FindByIdsAndAvailableAsync(productIds);

                if (products.Count != productIds.Count)
                    throw new NotFoundException("Một số sản phẩm không tồn tại hoặc không còn kinh doanh.");

                var productDict = products.ToDictionary(p => p.Id);

                // ================== CẬP NHẬT: ĐƯA TRỪ TỒN KHO BATCH RA NGOÀI VÒNG LẶP ==================
                var decrementResults = await _unitOfWork.ProductRepository.DecrementStockBatchAsync(requestProducts);
                var failedIds = decrementResults.Where(r => r.IsUpdated == 0).Select(r => r.ProductId).ToList();

                if (failedIds.Count > 0)
                {
                    var failedName = productDict[failedIds[0]].Name;
                    throw new BadRequestException($"Sản phẩm '{failedName}' hiện không đủ tồn kho");
                }

                foreach (var item in groupedItems)
                {
                    var product = productDict[item.ProductId];

                    subTotal += product.Price * item.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        //Product = product // Tối ưu RAM cho việc map dữ liệu trả về FE
                    });

                    checkoutCommands.Add(new CheckoutItemCommand
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        CategoryId = product.CategoryId,
                        BrandId = product.BrandId
                    });
                }

                // Tính toán Promotion
                var codesToProcess = promotionCodes ?? new List<string>();

                var promoResult = await _promotionService.CalculatePromotionAsync(
                    codesToProcess, checkoutCommands, subTotal, userId, trackingPhone);

                await ProcessPromotionUsages(
                    orderId, userId, trackingPhone, receiverFullName, promoResult, orderItems, chosenFreeProductIds);

                // TẠO ĐƠN HÀNG
                var order = CreateOrderObject(
                    orderId, userId, deliveryType, subTotal, promoResult.TotalDiscountAmount,
                    receiverEmail, receiverFullName, shippingAddress, trackingPhone, paidType,
                    receiverIdentityCard, installmentDurationMonth, notes, orderItems
                );

                await _unitOfWork.OrderRepository.AddOrderAsync(order);

                // Tối ưu Query: Không lấy lại Installments từ Database
                var installmentList = new List<Installment>();
                if (paidType == PaidType.Installment)
                {
                    installmentList = await CreateInstallmentRecords(order, installmentDurationMonth!.Value);
                }

                // Thực thi các hành động dọn dẹp (Xóa Cart, Xóa CustomPC...) nếu có
                if (onBeforeSaveAsync != null)
                {
                    await onBeforeSaveAsync();
                }

                // Lưu thay đổi và commit
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var finalOrder = await GetOrderDetailsAsync(orderId);
                var usageList = await _unitOfWork.PromotionUsageRepository.GetByOrderIdIncludePromotionAsync(orderId);

                return (finalOrder, installmentList, usageList);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // ============================== PRIVATE HELPERS ===============================
        private async Task ProcessPromotionUsages(
            Guid orderId,
            Guid? userId,
            string phone,
            string fullName,
            PromotionCalculationResult result,
            List<OrderItem> orderItems,
            List<Guid>? chosenFreeProductIds)
        {
            // ===== LOAD TẤT CẢ PROMOTION 1 LẦN (TRÁNH N+1) =====
            var promoIds = result.AppliedPromotions
                .Select(p => p.PromotionId)
                .Distinct()
                .ToList();

            var promotions = await _unitOfWork.PromotionRepository
                .FindByIdsIncludeAppliedProductsAsync(promoIds);

            var promoDict = promotions.ToDictionary(p => p.Id);

            // ===== LOAD USAGE COUNT 1 LẦN =====
            Dictionary<Guid, int> usageDict;

            if (userId.HasValue)
            {
                usageDict = await _unitOfWork.PromotionUsageRepository
                    .CountByPromotionIdsAndUserIdAsync(promoIds, userId.Value);
            }
            else
            {
                usageDict = await _unitOfWork.PromotionUsageRepository
                    .CountByPromotionIdsAndPhoneAsync(promoIds, phone);
            }

            // ===== TẠO LIST USAGES (BATCH INSERT) =====
            var usageList = new List<PromotionUsage>();

            foreach (var applied in result.AppliedPromotions)
            {
                if (!promoDict.TryGetValue(applied.PromotionId, out var promo))
                    throw new NotFoundException($"Không tìm thấy khuyến mãi '{applied.PromotionCode}'.");

                // ===== CHECK MAX USAGE PER USER =====
                if (promo.MaxUsagePerUser.HasValue)
                {
                    usageDict.TryGetValue(applied.PromotionId, out var usageCount);

                    if (usageCount >= promo.MaxUsagePerUser.Value)
                        throw new BadRequestException($"Bạn đã hết lượt dùng mã '{applied.PromotionCode}'.");
                }

                // ===== ATOMIC SYSTEM USAGE COUNT =====
                var affectedRows = await _unitOfWork.PromotionRepository
                    .IncrementUsageCountIfMaxUsageNotExceed(applied.PromotionId);

                if (affectedRows == 0)
                    throw new BadRequestException($"Khuyến mãi '{applied.PromotionCode}' đã đạt giới hạn hệ thống.");

                // ===== CREATE USAGE =====
                usageList.Add(new PromotionUsage
                {
                    PromotionId = applied.PromotionId,
                    UserId = userId,
                    OrderId = orderId,
                    FullName = fullName,
                    Phone = phone,
                    DiscountAmount = applied.DiscountAmount
                });
            }

            // ===== INSERT ALL USAGES 1 LẦN =====
            if (usageList.Any())
                await _unitOfWork.PromotionUsageRepository.AddRangeAsync(usageList);

            // ===== HANDLE FREE ITEMS =====
            foreach (var freeItem in result.TotalFreeItems)
            {
                bool shouldAdd = false;

                if (chosenFreeProductIds != null && chosenFreeProductIds.Any())
                {
                    if (chosenFreeProductIds.Contains(freeItem.ProductId))
                        shouldAdd = true;
                }
                else
                {
                    shouldAdd = true;
                }

                if (shouldAdd)
                {
                    var affectedGiftStock = await _unitOfWork.ProductRepository
                        .DecrementStockAtomicAsync(freeItem.ProductId, freeItem.Quantity);

                    if (affectedGiftStock == 0)
                        throw new BadRequestException("Quà tặng vừa hết hàng.");

                    orderItems.Add(new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = freeItem.ProductId,
                        Quantity = freeItem.Quantity,
                        UnitPrice = 0
                    });
                }
            }
        }


        // Tạo đối tượng Order từ các tham số đã chuẩn bị
        private Order CreateOrderObject(Guid id, Guid? userId, DeliveryType deliveryType, decimal subTotal, decimal discountAmount,
                                      string? email, string name, string? address, string phone,
                                      PaidType paidType, string? idCard, int? duration, string? notes,
                                      List<OrderItem> items)
        {
            decimal shippingCost = (deliveryType == DeliveryType.Shipping ? 30000 : 0);
            decimal taxableAmount = Math.Max(0, subTotal - discountAmount);
            decimal tax = taxableAmount * 0.1m;

            return new Order
            {
                Id = id,
                UserId = userId,
                DeliveryType = deliveryType,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Tax = tax,
                DiscountAmount = discountAmount,
                Status = OrderStatus.Pending,
                OrderDate = DateTimeOffset.Now,
                Items = items,
                TotalPrice = Math.Max(0, subTotal + shippingCost + tax - discountAmount),
                ReceiverEmail = email,
                ReceiverFullName = name,
                ShippingAddress = address,
                TrackingPhone = phone,
                PaidType = paidType,
                ReceiverIdentityCard = idCard,
                InstallmentDurationMonth = (paidType == PaidType.Installment) ? duration : null,
                Notes = notes
            };
        }

        private async Task<List<Installment>> CreateInstallmentRecords(Order order, int duration)
        {
            var installments = new List<Installment>();

            decimal totalPrice = order.TotalPrice;
            decimal firstMonthAmount = Math.Round(totalPrice * 0.20m, 0);
            decimal remainingTotal = totalPrice - firstMonthAmount;
            decimal monthlyAmountForRest = Math.Round(remainingTotal / (duration - 1), 0);

            decimal allocatedTotal = 0;

            for (int i = 1; i <= duration; i++)
            {
                decimal currentAmount =
                    (i == 1)
                        ? firstMonthAmount
                        : (i == duration
                            ? totalPrice - allocatedTotal
                            : monthlyAmountForRest);

                allocatedTotal += currentAmount;

                installments.Add(new Installment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Period = i,
                    Amount = currentAmount,
                    Status = InstallmentStatus.Pending,
                    DueDate = DateTimeOffset.Now.AddMonths(i)
                });
            }

            // ===== INSERT BATCH =====
            await _unitOfWork.InstallmentRepository.AddRangeAsync(installments);

            return installments;
        }

        private void ValidateOrderRequirements(DeliveryType deliveryType, string? inputAddress, string? profileAddress,
                                             PaidType paidType, string? idCard, int? duration)
        {
            if (deliveryType == DeliveryType.Shipping && string.IsNullOrWhiteSpace(inputAddress) && string.IsNullOrWhiteSpace(profileAddress))
                throw new BadRequestException("Địa chỉ giao hàng là bắt buộc.");

            if (paidType == PaidType.Installment)
            {
                if (string.IsNullOrWhiteSpace(idCard)) throw new BadRequestException("CCCD là bắt buộc.");
                var validDurations = new[] { 6, 9, 12 };
                if (!duration.HasValue || !validDurations.Contains(duration.Value)) throw new BadRequestException("Kỳ hạn trả góp không hợp lệ.");
            }
            else if (duration.HasValue) throw new BadRequestException("Không chọn kỳ hạn cho trả thẳng.");
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

        public async Task<Pagination<Order>> HandleGetCustomerOrderListWithPaginationAsync(
                                    Guid customerId,
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
                    .FindCustomerOrdersPagedSortByTotalPriceAsync(customerId, page, pageSize, isDescending, search, status),

                _ => await _unitOfWork.OrderRepository
                    .FindCustomerOrdersPagedSortByOrderDateAsync(customerId, page, pageSize, isDescending, search, status)
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

        // ============================== UPDATE ORDER STATUS ===============================

        /// <summary>
        /// Cập nhật trạng thái đơn hàng theo luồng nghiệp vụ.
        /// Nhân viên / Khách hàng gọi API này để chuyển trạng thái.
        /// </summary>
        public async Task<Order> HandleUpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            ValidateStatusTransition(order, newStatus);

            // Nếu đích đến là Completed hoặc Installing, tự động xác định dựa trên PaidType
            if (newStatus == OrderStatus.Completed || newStatus == OrderStatus.Installing)
            {
                order.Status = order.PaidType == PaidType.Installment
                    ? OrderStatus.Installing
                    : OrderStatus.Completed;
            }
            else
            {
                order.Status = newStatus;
            }

            await _unitOfWork.SaveChangesAsync();

            return order;
        }

        /// <summary>
        /// Hủy đơn hàng. Chỉ được hủy trước trạng thái Processing.
        /// Hoàn lại 90% số tiền đã thanh toán.
        /// </summary>
        public async Task<Order> HandleCancelOrderAsync(Guid orderId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var order = await _unitOfWork.OrderRepository.FindByIdIncludeDetailsAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

                    if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                        throw new BadRequestException("Chỉ có thể hủy đơn hàng trước khi đóng gói (trạng thái Pending hoặc Confirmed).");

                    // Hoàn lại tồn kho
                    foreach (var item in order.Items)
                    {
                        await _unitOfWork.ProductRepository.IncrementStockAtomicAsync(item.ProductId, item.Quantity);
                    }

                    // Hoàn lại 90% số tiền đã thanh toán
                    var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
                    var successSum = payments.Where(p => p.Status == PaymentStatus.Success).Sum(p => p.Amount);

                    if (successSum > 0)
                    {
                        var refundAmount = Math.Round(successSum * 0.9m, 0);
                        var refundPayment = new Payment
                        {
                            OrderId = orderId,
                            InstallmentId = null,
                            Amount = refundAmount,
                            Method = PaymentMethod.Cash,
                            Status = PaymentStatus.Refunded,
                            PaymentDate = DateTimeOffset.Now
                        };
                        await _unitOfWork.PaymentRepository.AddAsync(refundPayment);
                    }

                    // Cập nhật trạng thái (dùng tracking entity)
                    var trackedOrder = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");
                    trackedOrder.Status = OrderStatus.Canceled;

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return trackedOrder;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        /// <summary>
        /// Kiểm tra chuyển trạng thái hợp lệ theo luồng nghiệp vụ.
        /// </summary>
        private void ValidateStatusTransition(Order order, OrderStatus newStatus)
        {
            var currentStatus = order.Status;

            bool isValid = (currentStatus, newStatus, order.DeliveryType) switch
            {
                // Confirmed → Processing (nhân viên đóng gói)
                (OrderStatus.Confirmed, OrderStatus.Processing, _) => true,

                // === SHIPPING FLOW ===
                // Processing → Shipping (nhân viên giao hàng tiếp nhận)
                (OrderStatus.Processing, OrderStatus.Shipping, DeliveryType.Shipping) => true,
                // Shipping → Delivered (giao hàng thành công)
                (OrderStatus.Shipping, OrderStatus.Delivered, DeliveryType.Shipping) => true,
                // Delivered → Completed/Installing (khách hàng xác nhận)
                (OrderStatus.Delivered, OrderStatus.Completed, DeliveryType.Shipping) => true,
                (OrderStatus.Delivered, OrderStatus.Installing, DeliveryType.Shipping) => true,

                // === PICKUP FLOW ===
                // Processing → ReadyForPickup (đơn hàng sẵn sàng)
                (OrderStatus.Processing, OrderStatus.ReadyForPickup, DeliveryType.PickUp) => true,
                // ReadyForPickup → PickedUp (khách đến nhận)
                (OrderStatus.ReadyForPickup, OrderStatus.PickedUp, DeliveryType.PickUp) => true,
                // PickedUp → Completed/Installing (nhân viên xác nhận hoàn thành)
                (OrderStatus.PickedUp, OrderStatus.Completed, DeliveryType.PickUp) => true,
                (OrderStatus.PickedUp, OrderStatus.Installing, DeliveryType.PickUp) => true,

                _ => false
            };

            if (!isValid)
                throw new BadRequestException(
                    $"Không thể chuyển trạng thái từ '{currentStatus}' sang '{newStatus}' " +
                    $"cho đơn hàng loại '{order.DeliveryType}'.");
        }
    }
}