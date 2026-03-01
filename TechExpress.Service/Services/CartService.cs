using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;

namespace TechExpress.Service.Services
{
    public class CartService
    {
        private readonly UnitOfWork _unitOfWork;

        public CartService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Cart> HandleGetCurrentCartAsync(Guid userId)
        {
            var cart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsThenIncludeProductThenIncludeImagesWithSplitQueryAsync(userId);

            if (cart == null)
            {
                return new Cart
                {
                    Id = Guid.Empty,
                    UserId = userId,
                    Items = []
                };
            }

            return cart;
        }

        public async Task<Cart> HandleAddProductToCartAsync(Guid userId, Guid productId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new BadRequestException("Số lượng phải lớn hơn 0.");
            }

            var product = await _unitOfWork.ProductRepository.FindByIdAsync(productId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

            if (product.Status != ProductStatus.Available)
            {
                throw new BadRequestException("Sản phẩm hiện không khả dụng.");
            }

            if (quantity > product.Stock)
            {
                throw new BadRequestException($"Số lượng tồn kho không đủ. Chỉ còn {product.Stock} sản phẩm.");
            }

            var cart = await _unitOfWork.CartRepository.FindCartByUserIdWithTrackingAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
                await _unitOfWork.CartRepository.AddCartAsync(cart);
            }

            var existingItem = await _unitOfWork.CartItemRepository.FindCartItemByCartIdAndProductIdWithTrackingAsync(cart.Id, productId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = product.Price;
                existingItem.UpdatedAt = DateTimeOffset.Now;
            }
            else
            {
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                await _unitOfWork.CartItemRepository.AddCartItemAsync(cartItem);
            }
            cart.UpdatedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();

            var newCart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsThenIncludeProductThenIncludeImagesWithSplitQueryAsync(userId) ?? throw new NotFoundException($"Không tìm thấy giỏ hàng của người dùng {userId}");
            return newCart;
        }

        public async Task<Cart> HandleUpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int quantity)
        {
            if (quantity < 0)
            {
                throw new BadRequestException("Số lượng không được âm.");
            }

            var cart = await _unitOfWork.CartRepository.FindCartByUserIdWithTrackingAsync(userId)
                ?? throw new NotFoundException("Không tìm thấy giỏ hàng.");

            var cartItem = await _unitOfWork.CartItemRepository.FindCartItemByIdWithTrackingAsync(cartItemId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

            if (cartItem.CartId != cart.Id)
            {
                throw new ForbiddenException("Bạn không có quyền cập nhật sản phẩm này.");
            }

            if (quantity == 0)
            {
                _unitOfWork.CartItemRepository.RemoveCartItem(cartItem);
            }
            else
            {
                var product = await _unitOfWork.ProductRepository.FindByIdAsync(cartItem.ProductId)
                    ?? throw new NotFoundException("Không tìm thấy sản phẩm.");

                if (product.Status != ProductStatus.Available)
                {
                    throw new BadRequestException("Sản phẩm hiện không khả dụng.");
                }

                if (quantity > product.Stock)
                {
                    throw new BadRequestException($"Số lượng tồn kho không đủ. Chỉ còn  {product.Stock}  sản phẩm.");
                }

                cartItem.Quantity = quantity;
                cartItem.UnitPrice = product.Price;
                cartItem.UpdatedAt = DateTimeOffset.Now;
            }
            cart.UpdatedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();

            var updatedCart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsThenIncludeProductThenIncludeImagesWithSplitQueryAsync(userId) ?? throw new NotFoundException($"Không tìm thấy giỏ hàng của người dùng {userId}");
            return updatedCart;
        }

        public async Task<Cart> HandleRemoveCartItemAsync(Guid userId, Guid cartItemId)
        {
            var cart = await _unitOfWork.CartRepository.FindCartByUserIdWithTrackingAsync(userId)
                ?? throw new NotFoundException("Không tìm thấy giỏ hàng.");

            var cartItem = await _unitOfWork.CartItemRepository.FindCartItemByIdWithTrackingAsync(cartItemId)
                ?? throw new NotFoundException("Không tìm thấy sản phẩm trong giỏ hàng.");

            if (cartItem.CartId != cart.Id)
            {
                throw new ForbiddenException("Bạn không có quyền xoá sản phẩm này.");
            }

            _unitOfWork.CartItemRepository.RemoveCartItem(cartItem);
            cart.UpdatedAt = DateTimeOffset.Now;
            await _unitOfWork.SaveChangesAsync();
            var removedCart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsThenIncludeProductThenIncludeImagesWithSplitQueryAsync(userId) ?? throw new NotFoundException($"Không tìm thấy giỏ hàng của người dùng {userId}");
            return removedCart;
        }

        public async Task<Cart> HandleClearCartAsync(Guid userId)
        {
            var cart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsWithTrackingAsync(userId)
                ?? throw new NotFoundException("Không tìm thấy giỏ hàng.");

            _unitOfWork.CartItemRepository.ClearCartItems((List<CartItem>)cart.Items);
            cart.UpdatedAt = DateTimeOffset.Now;

            await _unitOfWork.SaveChangesAsync();
            var removedCart = await _unitOfWork.CartRepository.FindCartByUserIdIncludeItemsThenIncludeProductThenIncludeImagesWithSplitQueryAsync(userId) ?? throw new NotFoundException($"Không tìm thấy giỏ hàng của người dùng {userId}");
            return removedCart;
        }

        public async Task<int> HandleGetTotalItemsFromCartAsync(Guid userId)
        {            
            var cart = await _unitOfWork.CartRepository.FindCartByUserIdAsync(userId) ?? throw new NotFoundException($"Không tìm thấy giỏ hàng");
            return await _unitOfWork.CartItemRepository.GetTotalItemsFromCartIdAsync(cart.Id);
        }

    }
}
