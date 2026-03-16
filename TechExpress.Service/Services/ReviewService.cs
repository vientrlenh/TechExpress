using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Models;
using TechExpress.Service.Contexts;
using TechExpress.Service.Enums;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class ReviewService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly UserContext _userContext;
        private readonly NotificationHelper _notificationHelper;

        public ReviewService(UnitOfWork unitOfWork, UserContext userContext, NotificationHelper notificationHelper)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _notificationHelper = notificationHelper;
        }

        // =========================
        // GET: danh sách review của sản phẩm (public)
        // =========================
        public async Task<Pagination<Review>> HandleGetProductReviewsAsync(
            Guid productId,
            int page,
            int pageSize,
            int? rating,
            bool? hasMedia,
            ReviewSortBy sortBy = ReviewSortBy.CreatedAt,
            SortDirection sortDirection = SortDirection.Desc,
            CancellationToken ct = default)
        {
            await EnsureProductExistsAsync(productId);

            var (items, totalCount) = await _unitOfWork.ReviewRepository.GetPagedByProductIdAsync(
                productId, page, pageSize, rating, hasMedia,
                sortBy == ReviewSortBy.Rating,
                sortDirection == SortDirection.Asc,
                ct);

            return new Pagination<Review>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // =========================
        // POST: tạo review — guest hoặc customer
        // =========================
        public async Task<Review> HandleCreateReviewAsync(
            Guid productId,
            string? fullName,
            string? phone,
            string comment,
            int rating,
            List<string>? mediaUrls,
            CancellationToken ct = default)
        {
            if (rating < 1 || rating > 5)
                throw new BadRequestException("Đánh giá phải từ 1 đến 5 sao.");

            if (string.IsNullOrWhiteSpace(comment))
                throw new BadRequestException("Nội dung đánh giá không được để trống.");

            await EnsureProductExistsAsync(productId);

            // Validate phone format nếu được truyền từ request
            if (!string.IsNullOrWhiteSpace(phone))
                ValidatePhoneFormat(phone.Trim());

            Guid? userId = null;
            string? resolvedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            string? resolvedFullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim();

            var userIdStr = _userContext.GetCurrentAuthenticatedUserIdIfExist();
            if (userIdStr != null && Guid.TryParse(userIdStr, out var parsedUserId))
            {
                // Luồng Customer: chỉ auto-fill từ profile nếu field chưa được truyền
                userId = parsedUserId;
                var user = await _unitOfWork.UserRepository.FindUserByIdAsync(parsedUserId);
                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(resolvedPhone))
                        resolvedPhone = user.Phone;
                    if (string.IsNullOrWhiteSpace(resolvedFullName))
                        resolvedFullName = $"{user.FirstName} {user.LastName}".Trim();
                }
            }
            else
            {
                // Luồng Guest: phone và fullName đều bắt buộc
                if (string.IsNullOrWhiteSpace(resolvedPhone))
                    throw new BadRequestException("Khách vãng lai phải cung cấp số điện thoại.");
                if (string.IsNullOrWhiteSpace(resolvedFullName))
                    throw new BadRequestException("Khách vãng lai phải cung cấp họ tên.");
            }

            var reviewId = Guid.NewGuid();

            var review = new Review
            {
                Id = reviewId,
                ProductId = productId,
                UserId = userId,
                FullName = resolvedFullName,
                Phone = resolvedPhone,
                Comment = comment.Trim(),
                Rating = rating,
                IsDeleted = false,
                Medias = mediaUrls?
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => new ReviewMedia { ReviewId = reviewId, MediaUrl = u.Trim() })
                    .ToList() ?? []
            };

            await _unitOfWork.ReviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Tạo notification cho tất cả admin users khi có review mới
            var admins = await _unitOfWork.UserRepository.FindAdminUsersAsync();
            if (admins.Any())
            {
                foreach (var admin in admins)
                {
                    await _notificationHelper.CreateNewReviewNotificationAsync(
                        admin.Id,
                        productId,
                        (await _unitOfWork.ProductRepository.FindByIdAsync(productId))?.Name ?? "Sản phẩm",
                        resolvedFullName ?? "Khách vãng lai"
                    );
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return review;
        }

        // =========================
        // DELETE: xóa mềm review của chính mình (Customer only)
        // =========================
        public async Task HandleDeleteReviewAsync(Guid reviewId, CancellationToken ct = default)
        {
            var userId = _userContext.GetCurrentAuthenticatedUserId();

            var review = await _unitOfWork.ReviewRepository.FindByIdWithTrackingAsync(reviewId)
                ?? throw new NotFoundException("Không tìm thấy đánh giá.");

            if (review.UserId != userId)
                throw new ForbiddenException("Bạn không có quyền xóa đánh giá này.");

            review.IsDeleted = true;
            review.UpdatedAt = DateTimeOffset.Now;

            await _unitOfWork.SaveChangesAsync();
        }

        // =========================
        // Helpers
        // =========================

        private async Task EnsureProductExistsAsync(Guid productId)
        {
            if (await _unitOfWork.ProductRepository.FindByIdAsync(productId) == null)
                throw new NotFoundException("Không tìm thấy sản phẩm.");
        }

        private static void ValidatePhoneFormat(string phone)
        {
            if (!Regex.IsMatch(phone, @"^\d{9,12}$"))
                throw new BadRequestException("Số điện thoại phải là chữ số và có độ dài từ 9 đến 12 ký tự.");
        }
    }
}
