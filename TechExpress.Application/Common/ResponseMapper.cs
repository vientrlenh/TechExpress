using Microsoft.Identity.Client;
using System;
using TechExpress.Application.Controllers;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Application.DTOs.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Services;
using TechExpress.Service.Utils;

namespace TechExpress.Application.Common;

public class ResponseMapper
{
    public static UserResponse MapToUserResponseFromUser(User user)
    {
        return new UserResponse
        (
            user.Id,
            user.Email,
            user.Role,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Gender,
            user.Address,
            user.Ward,
            user.Province,
            user.PostalCode,
            user.AvatarImage,
            user.Identity,
            user.Salary,
            user.Status,
            user.CreatedAt
        );
    }

    //============= Map to StaffDetailResponse =============//
    public static StaffDetailResponse MapToStaffDetailResponseFromUser(User user)
    {
        return new StaffDetailResponse
        (
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Address,
            user.Province,
            user.Identity,
            user.Salary,
            user.Status
        );
    }

    public static List<UserResponse> MapToUserResponseListFromUserList(List<User> users)
    {
        return users.Select(MapToUserResponseFromUser).ToList();
    }

    public static AuthResponse MapToAuthResponse(string accessToken, string refreshToken, User user)
    {
        return new AuthResponse
        (
            accessToken,
            refreshToken,
            user.Email,
            user.Role
        );
    }

    public static Pagination<StaffListResponse> MapToStaffListResponsePaginationFromUserPagination(Pagination<User> userPagination)
    {
        var staffResponses = userPagination.Items.Select(MapToStaffListResponseFromUser).ToList();

        return new Pagination<StaffListResponse>
        {
            Items = staffResponses,
            PageNumber = userPagination.PageNumber,
            PageSize = userPagination.PageSize,
            TotalCount = userPagination.TotalCount
        };
    }

    public static StaffListResponse MapToStaffListResponseFromUser(User user)
    {
        return new StaffListResponse
        (
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Salary,
            user.Status
        );
    }

    public static Pagination<UserResponse> MapToUserResponsePaginationFromUserPagination(Pagination<User> userPagination)
    {
        var userResponses = userPagination.Items.Select(MapToUserResponseFromUser).ToList();

        return new Pagination<UserResponse>
        {
            Items = userResponses,
            PageNumber = userPagination.PageNumber,
            PageSize = userPagination.PageSize,
            TotalCount = userPagination.TotalCount
        };
    }
    //======================= Map Update Staff Response =======================//
    public static UpdateStaffResponse MapToUpdateStaffResponse(User user)
    {
        return new UpdateStaffResponse
        (
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Address,
            user.Ward,
            user.Province,
            user.Identity,
            user.Salary
        );
    }

    // ======================= Map Category Response từ model => Application.Dtos.response =======================//
    public static CategoryResponse MapToCategoryResponseFromCategory(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.ParentCategoryId,
            category.Description,
            category.ImageUrl,
            category.IsDeleted,
            category.CreatedAt,
            category.UpdatedAt
        );
    }

    public static List<CategoryResponse> MapToCategoryResponseListFromCategories(List<Category> categories)
    {
        return categories.Select(MapToCategoryResponseFromCategory).ToList();
    }

    // ======================= Map ProductListResponse =======================//
    public static Pagination<ProductListResponse>
    MapToProductListResponsePaginationFromProductPagination(
        Pagination<Product> productPagination)
    {
        var productResponses = productPagination.Items
            .Select(product =>
            {
                var firstImageUrl = product.Images
                    .OrderBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault();

                return new ProductListResponse(
                    product.Id,
                    product.Name,
                    product.Sku,
                    product.CategoryId,
                    product.BrandId,
                    product.Category?.Name ?? string.Empty,
                    product.Price,
                    product.Stock,
                    product.WarrantyMonth,
                    product.Status,
                    firstImageUrl,
                    product.CreatedAt,
                    product.UpdatedAt
                );
            })
            .ToList();

        return new Pagination<ProductListResponse>
        {
            Items = productResponses,
            PageNumber = productPagination.PageNumber,
            PageSize = productPagination.PageSize,
            TotalCount = productPagination.TotalCount
        };
    }

        public static List<ProductListResponse> MapToProductListResponsesFromProducts(List<Product> products)
        {
            var productResponses = products.Select(product =>
            {
                var firstImageUrl = product.Images
                    .OrderBy(i => i.Id)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault();

                return new ProductListResponse(
                    product.Id,
                    product.Name,
                    product.Sku,
                    product.CategoryId,
                    product.BrandId,
                    product.Category?.Name ?? string.Empty,
                    product.Price,
                    product.Stock,
                    product.WarrantyMonth,
                    product.Status,
                    firstImageUrl,
                    product.CreatedAt,
                    product.UpdatedAt
                );
            })
            .ToList();
            return productResponses;
        }

        

    public static ProductDetailResponse MapToProductDetailResponseFromProduct(Product product)
    {
        var thumbnailUrl = product.Images
            .OrderBy(i => i.Id)
            .Select(i => i.ImageUrl)
            .ToList();

        var specResponses = product.SpecValues
            .OrderBy(sv => sv.Id)
            .Select(sv =>
            {
                var def = sv.SpecDefinition;

                string value = def.AcceptValueType switch
                {
                    SpecAcceptValueType.Text => sv.TextValue ?? string.Empty,
                    SpecAcceptValueType.Number => sv.NumberValue?.ToString() ?? string.Empty,
                    SpecAcceptValueType.Decimal => sv.DecimalValue?.ToString() ?? string.Empty,
                    SpecAcceptValueType.Bool => sv.BoolValue?.ToString() ?? string.Empty,
                    _ => string.Empty
                };

                return new ProductSpecValueResponse(
                    def.Id,
                    def.Name,
                    def.Unit,
                    def.Code,
                    def.AcceptValueType,
                    value
                );
            })
            .ToList();

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Sku,
            product.CategoryId,
            product.BrandId,
            product.Category?.Name ?? string.Empty,
            product.Price,
            product.Stock,
            product.WarrantyMonth,
            product.Status,
            product.Description,
            thumbnailUrl,
            product.CreatedAt,
            product.UpdatedAt,
            specResponses
        );
    }

    public static ProductPCDetailResponse MapToProductPCDetailResponseFromProduct(Product product, List<ComputerComponent> components)
    {
        var baseDetail = MapToProductDetailResponseFromProduct(product);

        var componentResponses = (components ?? [])
            .OrderBy(c => c.Id)
            .Select(c => new ProductPCComponentResponse(
                c.ComponentProductId,
                c.ComponentProduct?.Name ?? string.Empty,
                c.ComponentProduct?.Sku ?? string.Empty,
                c.Quantity
            ))
            .ToList();

        return new ProductPCDetailResponse(
            baseDetail.Id,
            baseDetail.Name,
            baseDetail.Sku,
            baseDetail.CategoryId,
            baseDetail.BrandId,
            baseDetail.CategoryName,
            baseDetail.Price,
            baseDetail.Stock,
            baseDetail.WarrantyMonth,
            baseDetail.Status,
            baseDetail.Description,
            baseDetail.ThumbnailUrl,
            baseDetail.CreatedAt,
            baseDetail.UpdatedAt,
            baseDetail.SpecValues,
            componentResponses
        );
    }

    public static PCDetailsWithCompatibilityWarningResponse MapToPCDetailsWithCompatibilityWarningResponse(Product product, List<ComputerComponent> components, List<string>? compatibilityWarning)
    {
        return new PCDetailsWithCompatibilityWarningResponse(
            MapToProductPCDetailResponseFromProduct(product, components),
            compatibilityWarning
        );
    }




    //======================= Map SpecDefinition Response =======================//
    public static SpecDefinitionResponse MapToSpecDefinitionResponseFromSpecDefinition(SpecDefinition specDefinition)
    {
        return new SpecDefinitionResponse
        (
            specDefinition.Id,
            specDefinition.Code,
            specDefinition.Name,
            specDefinition.CategoryId,
            specDefinition.Category?.Name ?? string.Empty,
            specDefinition.Unit,
            specDefinition.AcceptValueType,
            specDefinition.Description,
            specDefinition.IsRequired,
            specDefinition.IsDeleted,
            specDefinition.CreatedAt,
            specDefinition.UpdatedAt
        );
    }

    public static List<SpecDefinitionResponse> MapToSpecDefinitionResponseListFromSpecDefinitionList(List<SpecDefinition> specDefinitions)
    {
        return specDefinitions.Select(MapToSpecDefinitionResponseFromSpecDefinition).ToList();
    }

    public static Pagination<SpecDefinitionResponse> MapToSpecDefinitionResponsePaginationFromSpecDefinitionPagination(Pagination<SpecDefinition> specDefinitionPagination)
    {
        var specDefinitionResponses = specDefinitionPagination.Items.Select(MapToSpecDefinitionResponseFromSpecDefinition).ToList();

        return new Pagination<SpecDefinitionResponse>
        {
            Items = specDefinitionResponses,
            PageNumber = specDefinitionPagination.PageNumber,
            PageSize = specDefinitionPagination.PageSize,
            TotalCount = specDefinitionPagination.TotalCount
        };
    }

    public static BrandResponse MapToBrandResponseFromBrand(Brand brand)
    {
        return new BrandResponse
        (
            brand.Id,
            brand.Name,
            brand.ImageUrl,
            brand.CreatedAt,
            brand.UpdatedAt
        );
    }

    public static Pagination<BrandResponse> MapToBrandResponsePaginationFromBrandPagination(Pagination<Brand> brandPagination)
    {
        var brandResponses = brandPagination.Items.Select(MapToBrandResponseFromBrand).ToList();

        return new Pagination<BrandResponse>
        {
            Items = brandResponses,
            PageNumber = brandPagination.PageNumber,
            PageSize = brandPagination.PageSize,
            TotalCount = brandPagination.TotalCount
        };
    }

    //======================= Map Cart Response =======================//
    public static CartResponse MapToCartResponseFromCart(Cart cart)
    {
        var itemResponses = cart.Items
            .OrderByDescending(i => i.CreatedAt)
            .Select(item => new CartItemResponse
            {
                Id = item.Id,
                CartId = item.CartId,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductImage = item.Product?.Images?.OrderBy(img => img.Id).FirstOrDefault()?.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            })
            .ToList();

        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            TotalItems = cart.Items.Sum(i => i.Quantity),
            Items = itemResponses,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }

    // =======================
    // Payment / Installment (API)
    // =======================

    public static SetPaymentIntentResponse MapToSetPaymentIntentResponse(Guid orderId, PaymentMethod method)
    {
        return new SetPaymentIntentResponse
        {
            OrderId = orderId,
            PaidType = PaidType.Full,
            Method = method
        };
    }

    public static SetInstallmentIntentResponse MapToSetInstallmentIntentResponse(
        Guid orderId,
        int months,
        List<Installment> schedule)
    {
        return new SetInstallmentIntentResponse
        {
            OrderId = orderId,
            PaidType = PaidType.Installment,
            Months = months,
            Schedule = schedule
                .OrderBy(i => i.Period)
                .Select(i => new InstallmentItemResponse
                {
                    Id = i.Id,
                    Period = i.Period,
                    Amount = i.Amount,
                    Status = i.Status,
                    DueDate = i.DueDate
                })
                .ToList()
        };
    }

    /// <summary>
    /// Map Service OnlinePaymentInitResult -> API InitOnlinePaymentResponse
    /// </summary>
    public static InitOnlinePaymentResponse MapToInitOnlinePaymentResponse(OnlinePaymentInitResult init)
    {
        return new InitOnlinePaymentResponse
        {
            SessionId = init.SessionId,
            RedirectUrl = init.RedirectUrl ?? string.Empty
        };
    }

    /// <summary>
    /// Map Service GatewayCallbackResult -> API GatewayCallbackResponse
    /// </summary>
    public static GatewayCallbackResponse MapToGatewayCallbackResponse(GatewayCallbackResult result)
    {
        return new GatewayCallbackResponse
        {
            Ok = result.Ok
        };
    }

    // =======================
    // Payment records
    // =======================

    public static PaymentResponse MapToPaymentResponseFromPayment(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            InstallmentId = payment.InstallmentId,
            Amount = payment.Amount,
            Method = payment.Method,
            Status = payment.Status,
            PaymentDate = payment.PaymentDate
        };
    }

    public static List<PaymentResponse> MapToPaymentResponseListFromPaymentList(List<Payment> payments)
    {
        return payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(MapToPaymentResponseFromPayment)
            .ToList();
    }

    /// <summary>
    /// Alias để bạn lỡ gọi MapToPaymentResponseList(...) trong controller vẫn chạy.
    /// </summary>
    public static List<PaymentResponse> MapToPaymentResponseList(List<Payment> payments)
    {
        return MapToPaymentResponseListFromPaymentList(payments);
    }

    public static CashPaymentResponse MapToCashPaymentResponseFromPayment(Payment payment)
    {
        return new CashPaymentResponse
        {
            PaymentId = payment.Id,
            Status = payment.Status,
            Method = payment.Method,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate
        };
    }

    // =======================
    // Installment records
    // =======================

    public static InstallmentResponse MapToInstallmentResponseFromInstallment(Installment installment)
    {
        return new InstallmentResponse
        {
            Id = installment.Id,
            OrderId = installment.OrderId,
            Period = installment.Period,
            Amount = installment.Amount,
            Status = installment.Status,
            DueDate = installment.DueDate
        };
    }

    public static List<InstallmentResponse> MapToInstallmentResponseListFromInstallmentList(List<Installment> installments)
    {
        return installments
            .OrderBy(i => i.Period)
            .Select(MapToInstallmentResponseFromInstallment)
            .ToList();
    }

    /// <summary>
    /// Alias để bạn lỡ gọi MapToInstallmentResponseList(...) vẫn chạy.
    /// </summary>
    public static List<InstallmentResponse> MapToInstallmentResponseList(List<Installment> installments)
    {
        return MapToInstallmentResponseListFromInstallmentList(installments);
    }

    // =======================
    // Refund
    // =======================

    public static RefundPaymentResponse MapToRefundPaymentResponse(long paymentId, string? reason)
    {
        return new RefundPaymentResponse
        {
            Ok = true,
            PaymentId = paymentId,
            Reason = reason
        };
    }

    public static CancelOrderRefundResponse MapToCancelOrderRefundResponse(CancelOrderRefundResult result)
    {
        return new CancelOrderRefundResponse
        {
            OrderId = result.OrderId,
            Status = result.Status,
            RefundAmount = result.RefundAmount,
            PayoutId = result.PayoutId,
            Reason = result.Reason,
            Message = result.Message
        };
    }







    //======================= Map Order Response =======================//
    // Cập nhật để tách PurchasedItems và GiftItems, hiện mã KM đã dùng
    public static OrderResponse MapToOrderResponseFromOrder(
        Order order,
        List<Installment>? installments = null,
        List<PromotionUsage>? promotionUsages = null)
    {
        var response = new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            Tax = order.Tax,
            TotalPrice = order.TotalPrice,
            DeliveryType = order.DeliveryType,
            PaidType = order.PaidType,
            ReceiverFullName = order.ReceiverFullName,
            ReceiverEmail = order.ReceiverEmail,
            ShippingAddress = order.ShippingAddress,
            TrackingPhone = order.TrackingPhone,
            Notes = order.Notes,
            ReceiverIdentityCard = order.ReceiverIdentityCard,
            InstallmentDurationMonth = order.InstallmentDurationMonth,

            // Ánh xạ danh sách khuyến mãi đã dùng từ tham số truyền vào
            AppliedPromotions = promotionUsages?.Select(pu => new AppliedPromotionResponse
            {
                PromotionId = pu.PromotionId,
                PromotionCode = pu.Promotion?.Code, // Cần đảm bảo Service đã nạp Promotion
                PromotionType = pu.Promotion!.Type, // Cần đảm bảo Service đã nạp Promotion và Promotion không null
                PromotionName = pu.Promotion?.Name,
                DiscountAmount = pu.DiscountAmount
            }).ToList() ?? new List<AppliedPromotionResponse>(),

            // Ánh xạ danh sách trả góp
            Installments = installments?.Select(i => new InstallmentResponse
            {
                Id = i.Id,
                Period = i.Period,
                OrderId = i.OrderId,
                Amount = i.Amount,
                Status = i.Status,
                DueDate = i.DueDate
            }).ToList() ?? new List<InstallmentResponse>()
        };

        // --- TÁCH BIỆT SẢN PHẨM MUA VÀ QUÀ TẶNG ---
        var allItemsMapped = order.Items.Select(oi => new OrderItemResponse
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.Product?.Name ?? "Sản phẩm không xác định",
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
        }).ToList();

        // Những món có giá > 0 là hàng mua
        response.PurchasedItems = allItemsMapped.Where(x => x.UnitPrice > 0).ToList();

        // Những món có giá = 0 là hàng tặng (Free Items)
        response.GiftItems = allItemsMapped.Where(x => x.UnitPrice == 0).ToList();

        return response;
    }

    public static OrderListItemResponse MapToOrderListItemResponseFromOrder(Order order)
    {
        return new OrderListItemResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount, // Cập nhật để hiện số tiền giảm ở list
            Tax = order.Tax,
            TotalPrice = order.TotalPrice,
            DeliveryType = order.DeliveryType,
            PaidType = order.PaidType,
            ReceiverFullName = order.ReceiverFullName,
            ReceiverEmail = order.ReceiverEmail,
            ShippingAddress = order.ShippingAddress,
            TrackingPhone = order.TrackingPhone,
            Notes = order.Notes,
            ReceiverIdentityCard = order.ReceiverIdentityCard,
            InstallmentDurationMonth = order.InstallmentDurationMonth
        };
    }


    public static OrderDetailResponse MapToOrderDetailResponseFromOrder(
        Order order,
        List<Installment>? installments,
        List<Payment>? payments)
    {
        return new OrderDetailResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            SubTotal = order.SubTotal,
            ShippingCost = order.ShippingCost,
            Tax = order.Tax,
            TotalPrice = order.TotalPrice,
            DeliveryType = order.DeliveryType,
            PaidType = order.PaidType,
            ReceiverFullName = order.ReceiverFullName,
            ReceiverEmail = order.ReceiverEmail,
            ShippingAddress = order.ShippingAddress,
            TrackingPhone = order.TrackingPhone,
            Notes = order.Notes,
            ReceiverIdentityCard = order.ReceiverIdentityCard,
            InstallmentDurationMonth = order.InstallmentDurationMonth,

            Items = order.Items.Select(oi =>
            {
                ProductListResponse? product = null;
                if (oi.Product != null)
                {
                    var firstImageUrl = oi.Product.Images
                        .OrderBy(i => i.Id)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault();

                    product = new ProductListResponse(
                        oi.Product.Id,
                        oi.Product.Name,
                        oi.Product.Sku,
                        oi.Product.CategoryId,
                        oi.Product.BrandId,
                        oi.Product.Category?.Name ?? string.Empty,
                        oi.Product.Price,
                        oi.Product.Stock,
                        oi.Product.WarrantyMonth,
                        oi.Product.Status,
                        firstImageUrl,
                        oi.Product.CreatedAt,
                        oi.Product.UpdatedAt
                    );
                }

                return new OrderItemDetailResponse
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Product = product
                };
            }).ToList(),

            Installments = installments == null
                ? new List<InstallmentResponse>()
                : MapToInstallmentResponseListFromInstallmentList(installments),

            Payments = payments == null
                ? new List<PaymentResponse>()
                : MapToPaymentResponseListFromPaymentList(payments)
        };
    }

    public static Pagination<OrderListItemResponse> MapToOrderListResponsePaginationFromOrderPagination(
        Pagination<Order> orderPagination)
    {
        var orderResponses = orderPagination.Items
            .Select(order => MapToOrderListItemResponseFromOrder(order))
            .ToList();

        return new Pagination<OrderListItemResponse>
        {
            Items = orderResponses,
            PageNumber = orderPagination.PageNumber,
            PageSize = orderPagination.PageSize,
            TotalCount = orderPagination.TotalCount
        };
    }

    //======================= Map Review Responses =======================//

    public static ReviewResponse MapToReviewResponse(Review review)
    {
        return new ReviewResponse
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            FullName = review.FullName,
            Phone = review.Phone,
            Comment = review.Comment,
            Rating = review.Rating,
            Medias = review.Medias
                .Select(m => new ReviewMediaResponse
                {
                    Id = m.Id,
                    MediaUrl = m.MediaUrl,
                    CreatedAt = m.CreatedAt
                }).ToList(),
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public static Pagination<ReviewResponse> MapToReviewResponsePagination(Pagination<Review> reviewPagination)
    {
        return new Pagination<ReviewResponse>
        {
            Items = reviewPagination.Items.Select(MapToReviewResponse).ToList(),
            PageNumber = reviewPagination.PageNumber,
            PageSize = reviewPagination.PageSize,
            TotalCount = reviewPagination.TotalCount
        };
    }

    //======================= Map Promotion Response =======================//
    public static PromotionResponse MapToPromotionResponseFromPromotion(Promotion promotion)
    {
        return new PromotionResponse(
            promotion.Id,
            promotion.Name,
            promotion.Code,
            promotion.Description,
            promotion.Type,
            promotion.Scope,
            promotion.DiscountValue,
            promotion.MaxDiscountValue,
            promotion.MinOrderValue,
            promotion.RequiredProducts
                .Select(rp => new PromotionRequiredProductResponse(rp.Id, rp.ProductId, rp.MinQuantity, rp.MaxQuantity))
                .ToList(),
            promotion.RequiredProductLogic,
            promotion.FreeProducts
                .Select(fp => new PromotionFreeProductResponse(fp.Id, fp.ProductId, fp.Quantity))
                .ToList(),
            promotion.FreeItemPickCount,
            promotion.CategoryId,
            promotion.BrandId,
            promotion.AppliedProducts
                .Select(ap => new PromotionAppliedProductResponse(ap.Id, ap.ProductId))
                .ToList(),
            promotion.MinAppliedQuantity,
            promotion.MaxUsageCount,
            promotion.UsageCount,
            promotion.MaxUsagePerUser,
            promotion.StartDate,
            promotion.EndDate,
            promotion.IsStackable,
            promotion.IsActive,
            promotion.CreatedAt,
            promotion.UpdatedAt
        );
    }

    // Map promotion từ promotion ra PromotionListResponse, thêm thuộc tính IsExpired để xác định xem khuyến mãi đã hết hạn hay chưa
    // Map từng item lẻ
    public static PromotionListResponse MapToPromotionListResponseFromPromotion(Promotion p)
    {
        return new PromotionListResponse(
            p.Id,
            p.Name,
            p.Code,
            p.Description,
            p.Type.ToString(),
            p.Scope.ToString(),
            p.DiscountValue,
            p.UsageCount,
            p.MaxUsageCount,
            p.IsActive,
            p.StartDate,
            p.EndDate,
            p.CreatedAt,
            DateTimeOffset.Now > p.EndDate // Attribute động tính tại thời điểm map
        );
    }

    // Map cả object Pagination (Dùng cho Controller)
    public static Pagination<PromotionListResponse> MapToPromotionListResponsePaginationFromPromotionPagination(Pagination<Promotion> promotionPagination)
    {
        return new Pagination<PromotionListResponse>
        {
            Items = promotionPagination.Items.Select(MapToPromotionListResponseFromPromotion).ToList(),
            PageNumber = promotionPagination.PageNumber,
            PageSize = promotionPagination.PageSize,
            TotalCount = promotionPagination.TotalCount
        };
    }

    public static CustomPCItemResponse MapToCustomPCItemResponseFromCustomPCItem(CustomPCItem item)
    {
        return new CustomPCItemResponse(item.Id, item.CustomPCId, item.ProductId, item.Quantity);
    }

    public static CustomPCResponse MapToCustomPCResponseFromCustomPC(CustomPC customPC)
    {
        return new CustomPCResponse
        (
            customPC.Id,
            customPC.UserId,
            customPC.Name,
            customPC.UpdatedAt,
            [..customPC.Items.Select(MapToCustomPCItemResponseFromCustomPCItem)]
        );
    }

    public static List<CustomPCResponse> MapToCustomPCResponseListFromCustomPCs(List<CustomPC> customPCs)
    {
        return [.. customPCs.Select(MapToCustomPCResponseFromCustomPC)];
    }

    public static ChatSessionResponse MapToChatSessionResponseFromChatSession(ChatSession session)
    {
        return new ChatSessionResponse(
            session.Id, 
            session.UserId, 
            session.FullName, 
            session.Phone, 
            session.IsClosed, 
            session.IsEscalated, 
            session.UpdatedAt);
    }

    public static ChatMessageResponse MapToChatMessageResponseFromChatMessage(ChatMessage message)
    {
        return new ChatMessageResponse(
            message.Id, 
            message.SessionId,
            message.SentById, 
            message.SentByFullName, 
            message.Message, 
            message.IsAiMessage, 
            MapToChatMediaResponseListFromChatMedias((List<ChatMedia>)message.Medias),
            message.CreatedAt
        );
    }

    public static List<ChatMessageResponse> MapToChatMessageResponsesFromChatMessages(List<ChatMessage> messages)
    {
        return [.. messages.Select(MapToChatMessageResponseFromChatMessage)];
    }

    public static ChatMessageResponseList MapToChatMessageResponseList(List<ChatMessage> messages, int currentPage, bool isMore)
    {
        return new ChatMessageResponseList(
            MapToChatMessageResponsesFromChatMessages(messages),
            currentPage,
            isMore
        );
    }

    public static ChatMediaResponse MapToChatMediaResponseFromChatMedia(ChatMedia media)
    {
        return new ChatMediaResponse(
            media.MediaUrl, 
            media.Type
        );
    }

    public static List<ChatMediaResponse> MapToChatMediaResponseListFromChatMedias(List<ChatMedia> medias)
    {
        return [.. medias.Select(MapToChatMediaResponseFromChatMedia)];
    }

    public static List<ChatSessionResponse> MapToChatSessionResponseListFromChatSessions(List<ChatSession> sessions)
    {
        return [.. sessions.Select(MapToChatSessionResponseFromChatSession)];
    }

    public static Pagination<ChatSessionResponse> MapToChatSessionPaginationResponse(Pagination<ChatSession> sessions)
    {
        return new Pagination<ChatSessionResponse>
        {
            Items = MapToChatSessionResponseListFromChatSessions(sessions.Items),
            PageNumber = sessions.PageNumber,
            PageSize = sessions.PageSize,
            TotalCount = sessions.TotalCount 
        };
    }

    public static PromotionDetailResponse MapToPromotionDetailResponseFromPromotion(Promotion promotion)
    {
        var now = DateTimeOffset.Now;

        return new PromotionDetailResponse
        {
            Id = promotion.Id,
            Name = promotion.Name,
            Code = promotion.Code,
            Description = promotion.Description,
            DiscountType = promotion.Type,
            DiscountValue = promotion.DiscountValue,
            MaxDiscountValue = promotion.MaxDiscountValue,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            UsageLimit = promotion.MaxUsageCount,
            UsagePerUser = promotion.MaxUsagePerUser,
            Status = promotion.IsActive,
            IsExpired = promotion.EndDate <= now,
            CreatedAt = promotion.CreatedAt,
            UpdatedAt = promotion.UpdatedAt,
            Scope = promotion.Scope,
            MinOrderValue = promotion.MinOrderValue,
            CategoryId = promotion.CategoryId,
            BrandId = promotion.BrandId,
            MinAppliedQuantity = promotion.MinAppliedQuantity,
            RequiredProductLogic = promotion.RequiredProductLogic,
            FreeItemPickCount = promotion.FreeItemPickCount,
            IsStackable = promotion.IsStackable,
            UsageCount = promotion.UsageCount,

           
        };



    }

    public static WarrantyCheckResponse MapToWarrantyCheckResponseFromResult(WarrantyCheckResult result)
    {
        return new WarrantyCheckResponse
        {
            OrderItemId = result.OrderItemId,
            ProductName = result.ProductName,
            ProductSku = result.ProductSku,
            WarrantyStartDate = result.WarrantyStartDate,
            WarrantyMonths = result.WarrantyMonths,
            WarrantyExpiredAt = result.WarrantyExpiredAt,
            CheckedAt = result.CheckedAt,
            IsValid = result.IsValid,
            RemainingDays = result.RemainingDays,
            Message = result.Message,
            TicketId = result.TicketId,
            MessageId = result.MessageId
        };
    }
}
