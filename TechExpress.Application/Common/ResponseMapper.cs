using Microsoft.Identity.Client;
using System;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Application.DTOs.Responses;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
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

    //======================= Map Order Response =======================//
    public static OrderResponse MapToOrderResponseFromOrder(Order order, Installment? installment = null)
    {
        return new OrderResponse
        {
            Id = order.Id,
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
            Items = order.Items.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Sản phẩm không xác định",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList(),

            // Ánh xạ duy nhất 1 bản ghi summary trả về cho Client
            Installment = installment != null ? new InstallmentResponse
            {
                Id = installment.Id,
                Period = installment.Period,
                Amount = installment.Amount,
                Status = installment.Status,
                DueDate = installment.DueDate // Đây là ngày kết thúc kỳ hạn
            } : null
        };
    }
}