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
        private readonly NotificationHelper _notificationHelper;

        public OrderService(UnitOfWork unitOfWork, UserContext userContext, PromotionService promotionService, NotificationHelper notificationHelper)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _promotionService = promotionService;
            _notificationHelper = notificationHelper;
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

                var user = await _unitOfWork.UserRepository.FindUserByIdWithTrackingAsync(userId)
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

                    //user.Phone = trackingPhone;
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

                // GỌI HÀM HELPER VÀ DÙNG onBeforeSaveAsync ĐỂ ĐẢM BẢO ATOMIC (CÙNG 1 TRANSACTION)
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId, rawItems, promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes,
                    onBeforeSaveAsync: async () =>
                    {
                        // Đoạn code này sẽ được Helper gọi ngay trước khi nó SaveChanges() và Commit()
                        foreach (var item in selectedItems)
                        {
                            _unitOfWork.CartItemRepository.RemoveCartItem(item);
                        }
                        await Task.CompletedTask;
                    }
                );
            }); // Kết thúc strategy.ExecuteAsync
        }
        // ============================== CUSTOM PC CHECKOUT (DÙNG CHUNG CHO CẢ GUEST VÀ MEMBER) ===============================
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleCustomPCCheckoutAsync(
            Guid? userId, // ĐỔI THÀNH Guid? ĐỂ NHẬN GUEST
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
                var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId)
                    ?? throw new NotFoundException("Không tìm thấy cấu hình PC này.");

                bool isOwner = false;

                // ================== LOGIC KIỂM TRA QUYỀN RẼ NHÁNH ==================
                if (userId.HasValue)
                {
                    // TRƯỜNG HỢP 1: NGƯỜI DÙNG ĐÃ ĐĂNG NHẬP
                    if (customPC.UserId.HasValue)
                    {
                        isOwner = (customPC.UserId.Value == userId.Value);
                    }
                    else
                    {
                        // PC tạo lúc chưa đăng nhập (Session), giờ đăng nhập rồi thanh toán
                        isOwner = !string.IsNullOrWhiteSpace(sessionId) && customPC.SessionId == sessionId;
                    }
                }
                else
                {
                    // TRƯỜNG HỢP 2: KHÁCH VÃNG LAI (GUEST) - KHÔNG ĐĂNG NHẬP
                    if (customPC.UserId.HasValue)
                    {
                        // Nếu PC này đã có chủ (có UserId), Guest không được phép thanh toán
                        throw new ForbiddenException("Cấu hình PC này thuộc về tài khoản thành viên. Vui lòng đăng nhập để thanh toán.");
                    }

                    isOwner = !string.IsNullOrWhiteSpace(sessionId) && customPC.SessionId == sessionId;
                }

                if (!isOwner)
                {
                    throw new ForbiddenException("Bạn không có quyền thanh toán cấu hình PC này.");
                }
                // ====================================================================

                if (customPC.Items == null || !customPC.Items.Any())
                    throw new BadRequestException("Cấu hình PC này chưa có linh kiện nào.");

                // ================== XỬ LÝ THÔNG TIN GIAO HÀNG ==================
                User? user = null;
                if (userId.HasValue)
                {
                    user = await _unitOfWork.UserRepository.FindUserByIdWithTrackingAsync(userId.Value)
                        ?? throw new NotFoundException("Người dùng không tồn tại.");
                }

                var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName)
                    ? receiverFullName
                    : (user != null ? $"{user.FirstName} {user.LastName}".Trim() : null);

                if (string.IsNullOrWhiteSpace(finalFullName))
                    throw new BadRequestException("Họ tên người nhận là bắt buộc.");

                var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : user?.Email;
                var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : user?.Address;

                string finalPhone;
                if (user != null)
                {
                    // Logic check số điện thoại cho Member
                    if (string.IsNullOrWhiteSpace(user.Phone) && string.IsNullOrWhiteSpace(trackingPhone))
                        throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                    if (!string.IsNullOrWhiteSpace(user.Phone) && !string.IsNullOrWhiteSpace(trackingPhone) && user.Phone != trackingPhone)
                        throw new BadRequestException("Số điện thoại không khớp với hồ sơ.");

                    finalPhone = string.IsNullOrWhiteSpace(user.Phone) ? trackingPhone! : user.Phone;
                    //user.Phone = finalPhone;
                }
                else
                {
                    // Logic check số điện thoại cho Guest
                    if (string.IsNullOrWhiteSpace(trackingPhone))
                        throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                    finalPhone = trackingPhone;
                }

                var rawItems = customPC.Items.Select(ci => (ci.ProductId, ci.Quantity)).ToList();

                // Sử dụng Hàm Helper Dùng Chung
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId: userId, // Vẫn truyền thẳng userId (có thể là Guid hoặc null)
                    rawItems: rawItems,
                    promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes
                );
            });
        }

        //================================ CUSTOMPC CHECKOUT CHO STAFF TẠO ORDER =============================
        public async Task<(Order order, List<Installment> installments, List<PromotionUsage> usages)> HandleCustomPCStaffCheckoutAsync(
            Guid staffId, // NHẬN ID CỦA NHÂN VIÊN TỪ CONTROLLER
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
                // Tìm cấu hình CustomPC cần thanh toán
                var customPC = await _unitOfWork.CustomPCRepository.FindByIdIncludeItemsAsync(customPCId)
                    ?? throw new NotFoundException("Không tìm thấy cấu hình PC này.");

                // LOGIC KIỂM TRA QUYỀN DÀNH RIÊNG CHO STAFF
                // Staff chỉ được phép thao tác nếu khách hàng đã bật cờ IsStaffAccessible
                if (!customPC.IsStaffAccessible)
                {
                    throw new BadRequestException("Khách hàng chưa cấp quyền (IsStaffAccessible) cho nhân viên truy cập cấu hình PC này.");
                }

                if (customPC.Items == null || !customPC.Items.Any())
                    throw new BadRequestException("Cấu hình PC này chưa có linh kiện nào.");

                // Tìm thông tin KHÁCH HÀNG (chủ thực sự của cấu hình PC)
                User? customer = null;
                if (customPC.UserId.HasValue)
                {
                    customer = await _unitOfWork.UserRepository.FindUserByIdWithTrackingAsync(customPC.UserId.Value);
                }

                // Xử lý thông tin người nhận (Ưu tiên dữ liệu Request -> Dữ liệu Khách hàng -> Mặc định)
                var finalFullName = !string.IsNullOrWhiteSpace(receiverFullName)
                    ? receiverFullName
                    : $"{customer?.FirstName} {customer?.LastName}".Trim();

                if (string.IsNullOrWhiteSpace(finalFullName))
                    finalFullName = "Khách hàng";

                var finalEmail = !string.IsNullOrWhiteSpace(receiverEmail) ? receiverEmail : customer?.Email;
                var finalAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : customer?.Address;

                string finalPhone;
                if (customer != null)
                {
                    // Trường hợp PC này của Khách đã có tài khoản
                    if (string.IsNullOrWhiteSpace(customer.Phone) && string.IsNullOrWhiteSpace(trackingPhone))
                        throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                    if (!string.IsNullOrWhiteSpace(customer.Phone) && !string.IsNullOrWhiteSpace(trackingPhone) && customer.Phone != trackingPhone)
                        throw new BadRequestException("Số điện thoại không khớp với hồ sơ khách hàng.");

                    finalPhone = string.IsNullOrWhiteSpace(customer.Phone) ? trackingPhone! : customer.Phone;

                    //customer.Phone = finalPhone;
                }
                else
                {
                    // Trường hợp PC này của Khách Vãng lai (Guest)
                    if (string.IsNullOrWhiteSpace(trackingPhone))
                        throw new BadRequestException("Số điện thoại liên lạc là bắt buộc.");

                    finalPhone = trackingPhone;
                }

                var rawItems = customPC.Items.Select(ci => (ci.ProductId, ci.Quantity)).ToList();

                // TẠO ĐƠN HÀNG (SỬ DỤNG HELPER CHUNG)
                // QUAN TRỌNG: Ghi nhận đơn hàng cho Khách VÀ gán ID Staff tạo đơn
                return await ExecuteCoreCheckoutInTransactionAsync(
                    userId: customPC.UserId, // Chủ sở hữu Đơn hàng là Khách
                    rawItems: rawItems,
                    promotionCodes, chosenFreeProductIds, deliveryType, finalEmail, finalFullName,
                    finalAddress, finalPhone, paidType, receiverIdentityCard, installmentDurationMonth, notes,
                    createdByStaffId: staffId // Ghi nhận ID Nhân viên vào cột CreatedByStaffId của đơn hàng
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
            Func<Task>? onBeforeSaveAsync = null, // <-- 1. THÊM LẠI THAM SỐ NÀY VÀO ĐÂY
            Guid? createdByStaffId = null) // Bổ sung tham số lưu lại người tạo đơn (nếu là Staff)
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

                // TẠO ĐƠN HÀNG VÀ GẮN CỜ NHÂN VIÊN
                var order = CreateOrderObject(
                    orderId, userId, deliveryType, subTotal, promoResult.TotalDiscountAmount,
                    receiverEmail, receiverFullName, shippingAddress, trackingPhone, paidType,
                    receiverIdentityCard, installmentDurationMonth, notes, orderItems,
                    createdByStaffId // Truyền ID nhân viên vào Helper tạo Object
                );

                await _unitOfWork.OrderRepository.AddOrderAsync(order);

                // Tối ưu Query: Không lấy lại Installments từ Database
                var installmentList = new List<Installment>();
                if (paidType == PaidType.Installment)
                {
                    installmentList = await CreateInstallmentRecords(order, installmentDurationMonth!.Value);
                }


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
                                      List<OrderItem> items,
                                      Guid? createdByStaffId = null) // Bổ sung cờ Nhân viên
        {
            decimal shippingCost = (deliveryType == DeliveryType.Shipping ? 30000 : 0);
            decimal taxableAmount = Math.Max(0, subTotal - discountAmount);
            decimal tax = taxableAmount * 0.1m;

            return new Order
            {
                Id = id,
                UserId = userId, // Đơn hàng sẽ thuộc về khách
                CreatedByStaffId = createdByStaffId, // Lưu lại lịch sử nhân viên nào thao tác tạo giúp
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
            var order = await _unitOfWork.OrderRepository.FindByIdIncludeItemsThenIncludeProductWithSplitQueryAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng này.");
            return order;
        }

        public async Task<(Order, List<Installment>, List<Payment>)> HandleProcessOrder(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException($"Không tìm thấy đơn hàng {orderId}");
            if (order.Status is not OrderStatus.Confirmed)
            {
                throw new BadRequestException("Trạng thái của đơn hàng hiện tại không hợp lệ để thực hiện đóng gói");
            }
            order.Status = OrderStatus.Processing;
            await _unitOfWork.SaveChangesAsync();

            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
        }

        public async Task<(Order, List<Installment>, List<Payment>)> HandleDeliverOrder(Guid orderId, Guid staffId, string? courierService, string? courierTrackingCode, bool isSelfDeliver)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.DeliveryType is not DeliveryType.Shipping)
            {
                throw new BadRequestException("Đơn hàng không phải thuộc loại vận chuyển");
            }
            if (order.Status is not OrderStatus.Processing)
            {
                throw new BadRequestException("Trạng thái của đơn hàng hiện tại không hợp lệ để thực hiện vận chuyển");
            }
            if (!isSelfDeliver)
            {
                if (courierService is null || courierTrackingCode is null)
                {
                    throw new BadRequestException("Đơn hàng bắt buộc nhập thông tin của bên vận chuyển");
                }
                order.CourierService = courierService;
                order.CourierTrackingCode = courierTrackingCode;
            }
            else
            {
                order.DeliveredById = staffId;
            }
            order.Status = OrderStatus.Shipping;
            await _unitOfWork.SaveChangesAsync();
            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
        }

        
        public async Task<(Order, List<Installment>, List<Payment>)> HandleCompleteDeliverOrder(Guid orderId, Guid staffId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.DeliveryType is not DeliveryType.Shipping)
            {
                throw new BadRequestException("Đơn hàng không phải thuộc loại vận chuyển");
            }
            if (order.Status is not OrderStatus.Shipping)
            {
                throw new BadRequestException("Trạng thái của đơn hàng hiện tại không hợp lệ để thực hiện vận chuyển");
            }
            if (order.DeliveredById is not null && order.DeliveredById != staffId)
            {
                throw new ForbiddenException("Bạn không có quyền thực hiện hoàn thành vận chuyển đơn hàng này");
            }
            order.Status = OrderStatus.Delivered;
            order.ReceivedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();
            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
        }

        public async Task<(Order, List<Installment>, List<Payment>)> HandleMarkOrderAsReadyForPickUp(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.DeliveryType is not DeliveryType.PickUp)
            {
                throw new BadRequestException("Đơn hàng không phải thuộc loại nhận trực tiếp tại quầy");
            }
            if (order.Status is not OrderStatus.Processing)
            {
                throw new BadRequestException("Đơn hàng hiện tại không ở trạng thái để thực hiện việc chuyển đổi trạng thái sang chuẩn bị nhận tại quầy");
            }
            order.Status = OrderStatus.ReadyForPickup;
            await _unitOfWork.SaveChangesAsync();
            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
        }


        public async Task<(Order, List<Installment>, List<Payment>)> HandleCompletePickUpOrder(Guid orderId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.DeliveryType is not DeliveryType.PickUp)
            {
                throw new BadRequestException("Đơn hàng không phải thuộc loại nhận trực tiếp tại quầy");
            }
            if (order.Status is not OrderStatus.ReadyForPickup)
            {
                throw new BadRequestException("Đơn hàng hiện tại không ở trạng thái để thực hiện việc chuyển đổi trạng thái sang đã nhận tại quầy");
            }
            order.Status = OrderStatus.PickedUp;
            order.ReceivedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();
            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
        }


        public async Task<(Order, List<Installment>, List<Payment>)> HandleCompleteOrder(Guid orderId, Guid userId)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId) ?? throw new NotFoundException("Không tìm thấy đơn hàng");
            if (order.Status is not OrderStatus.Delivered && order.Status is not OrderStatus.PickedUp)
            {
                throw new BadRequestException("Đơn hàng hiện tại chưa thể hoàn thành");
            }
            var user = await _unitOfWork.UserRepository.FindUserByIdAsync(userId) ?? throw new NotFoundException("Không tìm thấy người dùng đang đăng nhập");
            if (order.DeliveryType is DeliveryType.Shipping)
            {
                if (order.UserId is not null)
                {
                    if (order.UserId != userId) throw new ForbiddenException("Bạn không có quyền thực hiện hoàn thành đơn hàng này");
                }
                else
                {
                    if (user.IsCustomerUser()) throw new ForbiddenException("Chỉ có quản trị viên hoặc nhân viên mới có thể hoàn thành đơn hàng này");
                }
            }
            else
            {
                if (order.UserId is not null)
                {
                    if (user.IsCustomerUser() && order.UserId != userId) throw new ForbiddenException("Bạn không có quyền thực hiện hoàn thành trên đơn hàng này");
                }
                else
                {
                    if (user.IsCustomerUser()) throw new ForbiddenException("Chỉ có quản trị viên hoặc nhân viên mới có thể hoàn thành đơn hàng này");
                }

            }
            order.Status = OrderStatus.Completed;
            await _unitOfWork.SaveChangesAsync();
            var (updatedOrder, installments, payments) = await HandleGetOrderDetailAsync(orderId);
            return (updatedOrder, installments, payments);
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
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
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

                    // Tạo notification khi order bị hủy
                    if (trackedOrder.UserId.HasValue)
                    {
                        await _notificationHelper.CreateOrderNotificationAsync(trackedOrder.UserId.Value, orderId, OrderStatus.Canceled);
                        await _unitOfWork.SaveChangesAsync();
                    }

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

    }
}