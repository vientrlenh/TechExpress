using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TechExpress.Application.Common;
using TechExpress.Application.Dtos.Requests;
using TechExpress.Application.Dtos.Responses;
using TechExpress.Service;
using TechExpress.Service.Constants;
using TechExpress.Service.Contexts;
using TechExpress.Service.Hubs;

namespace TechExpress.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer,Staff,Admin")]
    public class CartController : ControllerBase
    {
        private readonly ServiceProviders _serviceProvider;
        private readonly UserContext _userContext;
        private readonly IHubContext<CartHub> _cartHubContext;

        public CartController(ServiceProviders serviceProvider, UserContext userContext, IHubContext<CartHub> cartHubContext)
        {
            _serviceProvider = serviceProvider;
            _userContext = userContext;
            _cartHubContext = cartHubContext;
        }

        /// <summary>
        /// Get current user's cart with all items
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCart()
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleGetCurrentCartAsync(userId);
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            return Ok(ApiResponse<CartResponse>.OkResponse(response));
        }

        /// <summary>
        /// Get list of cart items for the current user's active cart (Customer only)
        /// </summary>
        [HttpGet("items")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleGetCurrentCartAsync(userId);
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            
            if (cart.Id == Guid.Empty || cart.Items.Count == 0)
            {
                return Ok(ApiResponse<List<CartItemResponse>>.OkResponse([]));
            }
            return Ok(ApiResponse<List<CartItemResponse>>.OkResponse(response.Items));
        }

        /// <summary>
        /// Add a product to cart
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleAddProductToCartAsync(
                userId,
                request.ProductId,
                request.Quantity
            );
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            await SendCartChagesBySignalR(userId, response);
            return Ok(ApiResponse<CartResponse>.OkResponse(response));
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleUpdateCartItemQuantityAsync(
                userId,
                cartItemId,
                request.Quantity
            );
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            await SendCartChagesBySignalR(userId, response);
            return Ok(ApiResponse<CartResponse>.OkResponse(response));
        }

        /// <summary>
        /// Remove a cart item
        /// </summary>
        [HttpDelete("items/{cartItemId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleRemoveCartItemAsync(userId, cartItemId);
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            await SendCartChagesBySignalR(userId, response);
            return Ok(ApiResponse<CartResponse>.OkResponse(response));
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("clear")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var cart = await _serviceProvider.CartService.HandleClearCartAsync(userId);
            var response = ResponseMapper.MapToCartResponseFromCart(cart);
            await SendCartChagesBySignalR(userId, response);
            return Ok(ApiResponse<CartResponse>.OkResponse(response));
        }


        [HttpGet("total")]
        public async Task<IActionResult> GetTotalItemsFromCart()
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();
            var response = await _serviceProvider.CartService.HandleGetTotalItemsFromCartAsync(userId);
            return Ok(ApiResponse<int>.OkResponse(response));
        }


        private async Task SendCartChagesBySignalR(Guid userId, CartResponse response)
        {
            var totalItems = response.Items.Sum(ci => ci.Quantity);
            var user = userId.ToString();
            await _cartHubContext.Clients.User(user).SendAsync(SignalRMessageConstant.NewCartItemList, user, response.Items);
            await _cartHubContext.Clients.User(user).SendAsync(SignalRMessageConstant.CartItemQuantityUpdate, user, totalItems);
        }
    }
}
