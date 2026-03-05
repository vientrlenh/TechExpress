using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V1.Payouts.Batch;
using PayOS.Models.Webhooks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class PaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly RedisUtils _redisUtils;
        private readonly PayOsClient _payOs;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        private const string PayOsSessionPrefix = "payos:sess:";        // main session key: payos:sess:{sessionId}
        private const string PayOsIndexOrderCode = "payos:idx:ocode:";  // payos:idx:ocode:{orderCode} -> sessionId
        private const string PayOsIndexPayLink = "payos:idx:plink:";    // payos:idx:plink:{paymentLinkId} -> sessionId
        private const string PayOsCallbackLock = "payos:cb:";           // lock duplicate callback
        private const string PayOsReturnCancelLock = "payos:rc:";       // lock duplicate return/cancel

        private const string PayOsIndexOrder = "payos:idx:order:";       // payos:idx:order:{orderId} -> sessionId
        private const string PayOsIndexInstallment = "payos:idx:ins:";   // payos:idx:ins:{installmentId} -> sessionId

        public PaymentService(UnitOfWork unitOfWork, RedisUtils redisUtils, PayOsClient payOs)
        {
            _unitOfWork = unitOfWork;
            _redisUtils = redisUtils;
            _payOs = payOs;
        }

        // =========================
        // 1) CHECKOUT - INTENT
        // =========================
        public async Task<Order> HandleSetFullPaymentIntentAsync(
            Guid orderId,
            PaymentMethod method,
            CancellationToken ct = default)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (method is not (PaymentMethod.PayOs or PaymentMethod.VnPay or PaymentMethod.Cash))
                throw new BadRequestException("Phương thức thanh toán không hợp lệ.");

            order.PaidType = PaidType.Full;

            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        // =========================
        // 2) ONLINE INIT (REDIRECT)
        // =========================
        public async Task<OnlinePaymentInitResult> HandleInitOrderOnlinePaymentAsync(
    Guid orderId,
    PaymentMethod method,
    string? returnUrl,
    string? cancelUrl,
    CancellationToken ct = default)
        {
            if (method != PaymentMethod.PayOs)
                throw new BadRequestException("Bản hiện tại chỉ implement PayOS.");

            // ✅ REUSE nếu link còn hạn
            var reusable = await TryGetReusableSessionByOrderIdAsync(orderId);
            if (reusable != null)
            {
                return new OnlinePaymentInitResult
                {
                    SessionId = reusable.SessionId,
                    RedirectUrl = reusable.RedirectUrl!,
                    OrderCode = reusable.OrderCode,
                    PaymentLinkId = reusable.PaymentLinkId,
                    ExpiredAt = reusable.ExpiredAt
                };
            }

            // ===== create new link =====
            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.TotalPrice <= 0)
                throw new BadRequestException("Tổng tiền đơn hàng không hợp lệ.");

            var amount = ToVndInt(order.TotalPrice);

            var orderCode = CreateOrderCode();
            var expiredAt = DateTimeOffset.UtcNow.AddSeconds(_payOs.ExpirationSeconds).ToUnixTimeSeconds();

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;
            var finalCancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? _payOs.CancelUrl : cancelUrl;

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = "Thanh toán đơn",
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name = "Thanh toán đơn hàng",
                        Quantity = 1,
                        Price = amount
                    }
                },
                ReturnUrl = finalReturnUrl,
                CancelUrl = finalCancelUrl,
                ExpiredAt = expiredAt
            };

            var response = await _payOs.CreatePaymentLinkAsync(paymentRequest, ct);

            var session = new PayOsInitSession
            {
                SessionId = Guid.NewGuid(),
                OrderId = orderId,
                InstallmentId = null,
                PaidType = PaidType.Full,
                AmountVnd = amount,
                OrderCode = orderCode,
                PaymentLinkId = response.PaymentLinkId,
                RedirectUrl = response.CheckoutUrl,
                ExpiredAt = expiredAt 
            };

            await SaveSessionAsync(session);

            return new OnlinePaymentInitResult
            {
                SessionId = session.SessionId,
                RedirectUrl = session.RedirectUrl!,
                OrderCode = session.OrderCode,
                PaymentLinkId = session.PaymentLinkId,
                ExpiredAt = session.ExpiredAt
            };
        }
        public async Task<OnlinePaymentInitResult> HandleInitInstallmentOnlinePaymentAsync(
    Guid installmentId,
    PaymentMethod method,
    string? returnUrl,
    string? cancelUrl,
    CancellationToken ct = default)
        {
            if (method != PaymentMethod.PayOs)
                throw new BadRequestException("Bản hiện tại chỉ implement PayOS.");

            // ✅ REUSE nếu link còn hạn
            var reusable = await TryGetReusableSessionByInstallmentIdAsync(installmentId);
            if (reusable != null)
            {
                return new OnlinePaymentInitResult
                {
                    SessionId = reusable.SessionId,
                    RedirectUrl = reusable.RedirectUrl!,
                    OrderCode = reusable.OrderCode,
                    PaymentLinkId = reusable.PaymentLinkId,
                    ExpiredAt = reusable.ExpiredAt
                };
            }

            var installment = await _unitOfWork.InstallmentRepository.FindByIdAsync(installmentId)
                ?? throw new NotFoundException("Không tìm thấy kỳ trả góp.");

            var amount = ToVndInt(installment.Amount);

            var orderCode = CreateOrderCode();
            var expiredAt = DateTimeOffset.UtcNow.AddSeconds(_payOs.ExpirationSeconds).ToUnixTimeSeconds();

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;
            var finalCancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? _payOs.CancelUrl : cancelUrl;

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = $"Trả góp kỳ {installment.Period}",
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name = $"Thanh toán trả góp kỳ {installment.Period}",
                        Quantity = 1,
                        Price = amount
                    }
                },
                ReturnUrl = finalReturnUrl,
                CancelUrl = finalCancelUrl,
                ExpiredAt = expiredAt
            };

            var response = await _payOs.CreatePaymentLinkAsync(paymentRequest, ct);

            var session = new PayOsInitSession
            {
                SessionId = Guid.NewGuid(),
                OrderId = installment.OrderId,
                InstallmentId = installmentId,
                PaidType = PaidType.Installment,
                AmountVnd = amount,
                OrderCode = orderCode,
                PaymentLinkId = response.PaymentLinkId,
                RedirectUrl = response.CheckoutUrl,
                ExpiredAt = expiredAt 
            };

            await SaveSessionAsync(session);

            return new OnlinePaymentInitResult
            {
                SessionId = session.SessionId,
                RedirectUrl = session.RedirectUrl!,
                OrderCode = session.OrderCode,
                PaymentLinkId = session.PaymentLinkId,
                ExpiredAt = session.ExpiredAt
            };
        }

        /// <summary>
        /// Tất toán: Thanh toán toàn bộ số tiền còn lại của đơn hàng trả góp trước thời hạn.
        /// </summary>
        public async Task<OnlinePaymentInitResult> HandleInitFullSettlementAsync(
            Guid orderId,
            PaymentMethod method,
            string? returnUrl,
            string? cancelUrl,
            CancellationToken ct = default)
        {
            if (method != PaymentMethod.PayOs)
                throw new BadRequestException("Bản hiện tại chỉ implement PayOS.");

            // ✅ REUSE nếu link còn hạn
            var reusable = await TryGetReusableSessionByOrderIdAsync(orderId);
            if (reusable != null && reusable.IsFullSettlement)
            {
                return new OnlinePaymentInitResult
                {
                    SessionId = reusable.SessionId,
                    RedirectUrl = reusable.RedirectUrl!,
                    OrderCode = reusable.OrderCode,
                    PaymentLinkId = reusable.PaymentLinkId,
                    ExpiredAt = reusable.ExpiredAt
                };
            }

            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.PaidType != PaidType.Installment)
                throw new BadRequestException("Chỉ có thể tất toán đơn hàng trả góp.");

            // Tính số tiền còn lại = Tổng tiền đơn hàng - Tổng số tiền đã thanh toán thành công
            var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            var totalPaid = payments
                .Where(p => p.Status == PaymentStatus.Success)
                .Sum(p => p.Amount);

            var remainingAmount = order.TotalPrice - totalPaid;

            if (remainingAmount <= 0)
                throw new BadRequestException("Đơn hàng đã được thanh toán đủ. Không cần tất toán.");

            var amount = ToVndInt(remainingAmount);
            var orderCode = CreateOrderCode();
            var expiredAt = DateTimeOffset.UtcNow.AddSeconds(_payOs.ExpirationSeconds).ToUnixTimeSeconds();

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;
            var finalCancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? _payOs.CancelUrl : cancelUrl;

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = $"Tất toán đơn hàng trả góp",
                Items = new List<PaymentLinkItem>
                {
                    new PaymentLinkItem
                    {
                        Name = $"Tất toán đơn hàng trả góp - Số tiền còn lại",
                        Quantity = 1,
                        Price = amount
                    }
                },
                ReturnUrl = finalReturnUrl,
                CancelUrl = finalCancelUrl,
                ExpiredAt = expiredAt
            };

            var response = await _payOs.CreatePaymentLinkAsync(paymentRequest, ct);

            var session = new PayOsInitSession
            {
                SessionId = Guid.NewGuid(),
                OrderId = orderId,
                InstallmentId = null, // null để đánh dấu đây là full settlement
                PaidType = PaidType.Installment,
                AmountVnd = amount,
                OrderCode = orderCode,
                PaymentLinkId = response.PaymentLinkId,
                RedirectUrl = response.CheckoutUrl,
                ExpiredAt = expiredAt,
                IsFullSettlement = true // Flag đánh dấu tất toán
            };

            await SaveSessionAsync(session);

            return new OnlinePaymentInitResult
            {
                SessionId = session.SessionId,
                RedirectUrl = session.RedirectUrl!,
                OrderCode = session.OrderCode,
                PaymentLinkId = session.PaymentLinkId,
                ExpiredAt = session.ExpiredAt
            };
        }

        // =========================
        // 3) GATEWAY CALLBACK (Controller đang gọi method này)
        // =========================
        public async Task<GatewayCallbackResult> HandlePayOsWebhookAsync(
             Webhook webhook,
             CancellationToken ct = default)
        {
            if (webhook == null) 
                throw new BadRequestException("Callback payload không hợp lệ.");

            // 1) Verify webhook với PayOS 2.0.1
            WebhookData verifiedData;
            try
            {
                verifiedData = await _payOs.VerifyWebhookAsync(webhook, ct);
            }
            catch (Exception ex)
            {
                return new GatewayCallbackResult 
                { 
                    Ok = false, 
                    Message = $"Webhook không hợp lệ: {ex.Message}" 
                };
            }

            // 2) Resolve session từ verified data
            var session = await ResolvePayOsSessionAsync(verifiedData);
            if (session == null)
                return new GatewayCallbackResult { Ok = false, Message = "Session không tồn tại hoặc đã hết hạn." };

            // 3) Idempotency lock (theo orderCode là hợp lý)
            var lockKey = $"{PayOsCallbackLock}{session.OrderCode}";
            var locked = await _redisUtils.TrySetStringIfNotExists(lockKey, "1", TimeSpan.FromDays(2));
            if (!locked)
                return new GatewayCallbackResult { Ok = true, Message = "Duplicate webhook (ignored)." };

            // 4) Xác định status và amount từ verified data
            // PayOS 2.0.1: WebhookData có thể có Status hoặc Success field
            var isSuccess = verifiedData.Code == "PAID" ;
            var status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
            var paidAmount = verifiedData.Amount > 0 ? (decimal)verifiedData.Amount / 100 : session.AmountVnd; // PayOS trả về amount theo đơn vị nhỏ nhất (cent), chia 100 để có VND

            // 5) Insert payment
            var payment = new Payment
            {
                OrderId = session.OrderId,
                InstallmentId = session.InstallmentId,
                Amount = paidAmount,
                Method = PaymentMethod.PayOs,
                Status = status,
                PaymentDate = DateTimeOffset.Now
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // 6) Update statuses nếu success
            if (status == PaymentStatus.Success)
            {
                if (session.IsFullSettlement)
                {
                    // Tất toán: đánh dấu tất cả các kỳ còn lại là Paid
                    await MarkAllRemainingInstallmentsAsPaidAsync(session.OrderId, ct);
                    await _unitOfWork.SaveChangesAsync();
                }
                else if (session.InstallmentId.HasValue)
                {
                    // Thanh toán từng kỳ: update installment by sum
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Cập nhật trạng thái đơn hàng (Pending -> Confirmed, Installing -> Completed nếu đã trả hết)
                await UpdateOrderStatusAfterPaymentAsync(session.OrderId, ct);
                await _unitOfWork.SaveChangesAsync();
            }

            // (optional) clear session luôn
            await ClearSessionAsync(session);

            return new GatewayCallbackResult
            {
                Ok = true,
                Payment = payment,
                Message = "PayOS webhook processed."
            };
        }

        private async Task<PayOsInitSession?> ResolvePayOsSessionAsync(WebhookData verifiedData)
        {
            // ưu tiên paymentLinkId
            if (!string.IsNullOrWhiteSpace(verifiedData.PaymentLinkId))
            {
                var sidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexPayLink}{verifiedData.PaymentLinkId}");
                if (Guid.TryParse(sidStr, out var sid))
                    return await GetSessionByIdAsync(sid);
            }

            // fallback orderCode
            if (verifiedData.OrderCode > 0)
                return await FindSessionByOrderCodeAsync(verifiedData.OrderCode);

            return null;
        }
        // =========================
        // 3b) PAYOS RETURN/CANCEL (NO OFFICIAL WEBHOOK YET)
        // Controller gọi method này trong /payments/payos/return & /payments/payos/cancel
        // =========================
        public async Task<GatewayCallbackResult> HandlePayOsReturnOrCancelAsync(
            bool isSuccess,
            long? orderCode,
            Guid? sessionId,
            CancellationToken ct = default)
        {
            if (!orderCode.HasValue && !sessionId.HasValue)
                throw new BadRequestException("orderCode hoặc sessionId là bắt buộc.");

            // 1) lấy session
            PayOsInitSession? session = null;

            if (sessionId.HasValue)
                session = await GetSessionByIdAsync(sessionId.Value);

            if (session == null && orderCode.HasValue)
                session = await FindSessionByOrderCodeAsync(orderCode.Value);

            if (session == null)
                return new GatewayCallbackResult { Ok = false, Message = "Session không tồn tại hoặc đã hết hạn." };

            // 2) Idempotency lock (1 orderCode xử lý 1 lần)
            var lockKey = $"{PayOsReturnCancelLock}{session.OrderCode}";
            var locked = await _redisUtils.TrySetStringIfNotExists(lockKey, "1", TimeSpan.FromDays(2));
            if (!locked)
                return new GatewayCallbackResult { Ok = true, Message = "Duplicate (ignored)." };

            var status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;

            // 3) tạo payment record
            var payment = new Payment
            {
                OrderId = session.OrderId,
                InstallmentId = session.InstallmentId,
                Amount = session.AmountVnd,
                Method = PaymentMethod.PayOs,
                Status = status,
                PaymentDate = DateTimeOffset.Now
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();



            if (status == PaymentStatus.Success)
            {
                if (session.IsFullSettlement)
                {
                    // Tất toán qua return/cancel flow (tạm thời dùng session.AmountVnd)
                    await MarkAllRemainingInstallmentsAsPaidAsync(session.OrderId, ct);
                    await _unitOfWork.SaveChangesAsync();
                }
                else if (session.InstallmentId.HasValue)
                {
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);
                    await _unitOfWork.SaveChangesAsync();
                }

                await UpdateOrderStatusAfterPaymentAsync(session.OrderId, ct);
                await _unitOfWork.SaveChangesAsync();
            }

            await ClearSessionAsync(session);

            return new GatewayCallbackResult
            {
                Ok = true,
                Payment = payment,
                Message = isSuccess ? "Return processed (Success)." : "Cancel processed (Failed)."
            };
        }
        // =========================
        // 4) CASH/COD
        // =========================
        public async Task<Payment> HandleCashPayOrderAsync(
    Guid orderId,
    decimal amount,
    string? note,
    CancellationToken ct = default)
        {
            if (amount <= 0) throw new BadRequestException("Amount must be greater than 0.");

            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            var payment = new Payment
            {
                OrderId = orderId,
                InstallmentId = null,
                Amount = amount,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Success,
                PaymentDate = DateTimeOffset.Now
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            await UpdateOrderStatusAfterPaymentAsync(orderId, ct);
            await _unitOfWork.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment> HandleCashPayInstallmentAsync(
    Guid installmentId,
    decimal amount,
    string? note,
    CancellationToken ct = default)
        {
            if (amount <= 0) throw new BadRequestException("Amount must be greater than 0.");

            var installment = await _unitOfWork.InstallmentRepository.FindByIdWithTrackingAsync(installmentId)
                ?? throw new NotFoundException("Không tìm thấy kỳ trả góp.");

            var payment = new Payment
            {
                OrderId = installment.OrderId,
                InstallmentId = installmentId,
                Amount = amount,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Success,
                PaymentDate = DateTimeOffset.Now
            };

            await _unitOfWork.PaymentRepository.AddAsync(payment);

            await _unitOfWork.SaveChangesAsync();

            await UpdateInstallmentPaidStatusBySumAsync(installmentId, ct);
            await UpdateOrderStatusAfterPaymentAsync(installment.OrderId, ct);

            await _unitOfWork.SaveChangesAsync();

            return payment;
        }

        // =========================
        // 5) QUERY
        // =========================
        public Task<List<Payment>> HandleGetPaymentsByOrderAsync(Guid orderId, CancellationToken ct = default)
            => _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);

        public Task<List<Payment>> HandleGetPaymentsByInstallmentAsync(Guid installmentId, CancellationToken ct = default)
            => _unitOfWork.PaymentRepository.GetByInstallmentIdAsync(installmentId);

        // =========================
        // 6) REFUND & CANCEL ORDER
        // =========================
        

        /// <summary>
        /// Hủy đơn hàng và hoàn tiền 90% số tiền đã thanh toán.
        /// Chỉ có thể hủy trước khi đơn hàng ở trạng thái Processing.
        /// </summary>
        public async Task<CancelOrderRefundResult> HandleCancelOrderAndRefundAsync(
            Guid orderId,
            string toBin,
            string toAccountNumber,
            string? reason,
            CancellationToken ct = default)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            // Chỉ có thể hủy trước khi đơn hàng ở trạng thái Processing
            if (order.Status >= OrderStatus.Processing)
                throw new BadRequestException($"Không thể hủy đơn hàng. Trạng thái hiện tại: {order.Status}. Chỉ có thể hủy trước khi đóng gói (Processing).");

            // Kiểm tra đơn hàng đã bị hủy chưa
            if (order.Status == OrderStatus.Canceled)
                throw new BadRequestException("Đơn hàng đã bị hủy trước đó.");

            // Lấy tất cả payments thành công của đơn hàng
            var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            var successPayments = payments.Where(p => p.Status == PaymentStatus.Success).ToList();

            if (!successPayments.Any())
                throw new BadRequestException("Đơn hàng chưa có thanh toán thành công nào.");

            // Tính tổng số tiền đã thanh toán thành công
            var totalPaid = successPayments.Sum(p => p.Amount);

            // Hoàn lại 90% số tiền đã thanh toán
            var refundAmount = totalPaid * 0.9m;
            var refundAmountVnd = ToVndInt(refundAmount);

            // Tạo payout batch request
            var referenceId = $"refund_{orderId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var payoutRequest = new PayoutBatchRequest
            {
                ReferenceId = referenceId,
                Category = new List<string> { "refund" },
                ValidateDestination = true,
                Payouts = new List<PayoutBatchItem>
                {
                    new PayoutBatchItem
                    {
                        ReferenceId = $"{referenceId}_1",
                        Amount = refundAmountVnd,
                        Description = $"Hoàn tiền hủy đơn hàng {orderId}",
                        ToBin = toBin,
                        ToAccountNumber = toAccountNumber
                    }
                }
            };

            string? payoutId = null;
            try
            {
                // Gọi PayOS payout
                var payoutResponse = await _payOs.CreatePayoutBatchAsync(payoutRequest, ct);
                payoutId = payoutResponse.Id;
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Không thể thực hiện hoàn tiền qua PayOS: {ex.Message}");
            }

            // Restock lại sản phẩm
            var orderItems = await _unitOfWork.OrderRepository.FindByIdIncludeDetailsAsync(orderId);
            if (orderItems?.Items != null)
            {
                foreach (var item in orderItems.Items)
                {
                    await _unitOfWork.ProductRepository.IncrementStockAtomicAsync(item.ProductId, item.Quantity);
                }
            }

            // Cập nhật trạng thái đơn hàng thành Canceled
            order.Status = OrderStatus.Canceled;
            await _unitOfWork.SaveChangesAsync();

            return new CancelOrderRefundResult
            {
                OrderId = orderId,
                Status = order.Status,
                RefundAmount = refundAmount,
                PayoutId = payoutId,
                Reason = reason,
                Message = $"Đơn hàng đã được hủy và hoàn lại {refundAmount:N0} VND (90% số tiền đã thanh toán)."
            };
        }

        // ==========================================================
        // ===================== INTERNAL HELPERS ====================
        // ==========================================================

        private async Task SaveSessionAsync(PayOsInitSession session)
        {
            var ttl = TimeSpan.FromMinutes(_payOs.SessionTtlMinutes);
            var prefix = string.IsNullOrWhiteSpace(_payOs.RedisKeyPrefix) ? PayOsSessionPrefix : _payOs.RedisKeyPrefix;

            var json = JsonSerializer.Serialize(session, JsonOpts);

            // main key
            await _redisUtils.StoreStringData($"{prefix}{session.SessionId}", json, ttl);

            // index by paymentLinkId
            if (!string.IsNullOrWhiteSpace(session.PaymentLinkId))
            {
                await _redisUtils.StoreStringData(
                    $"{PayOsIndexPayLink}{session.PaymentLinkId}",
                    session.SessionId.ToString(),
                    TimeSpan.FromDays(2));
            }

            await _redisUtils.StoreStringData(
                $"{PayOsIndexOrderCode}{session.OrderCode}",
                session.SessionId.ToString(),
                TimeSpan.FromDays(2));

            await _redisUtils.StoreStringData(
                $"{PayOsIndexOrder}{session.OrderId}",
                session.SessionId.ToString(),
                ttl);

            if (session.InstallmentId.HasValue)
            {
                await _redisUtils.StoreStringData(
                    $"{PayOsIndexInstallment}{session.InstallmentId.Value}",
                    session.SessionId.ToString(),
                    ttl);
            }
        }

        private async Task<PayOsInitSession?> GetSessionByIdAsync(Guid sessionId)
        {
            var prefix = string.IsNullOrWhiteSpace(_payOs.RedisKeyPrefix) ? PayOsSessionPrefix : _payOs.RedisKeyPrefix;

            var json = await _redisUtils.GetStringDataFromKey($"{prefix}{sessionId}");
            if (string.IsNullOrWhiteSpace(json)) return null;

            return JsonSerializer.Deserialize<PayOsInitSession>(json, JsonOpts);
        }

        private async Task<PayOsInitSession?> FindSessionByOrderCodeAsync(long orderCode)
        {
            var sessionIdStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexOrderCode}{orderCode}");
            if (string.IsNullOrWhiteSpace(sessionIdStr) || !Guid.TryParse(sessionIdStr, out var sessionId))
                return null;

            return await GetSessionByIdAsync(sessionId);
        }

        // Update Order.Status dựa trên các Payment đã thanh toán thành công
        private async Task UpdateOrderStatusAfterPaymentAsync(Guid orderId, CancellationToken ct)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            // ===== INSTALLMENT FLOW =====
            if (order.PaidType == PaidType.Installment)
            {
                var schedule = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
                if (schedule != null && schedule.Count > 0)
                {
                    // 1) Chỉ khi kỳ đầu tiên (period nhỏ nhất) đã được thanh toán (Paid)
                    //    thì từ Pending mới chuyển sang Confirmed.
                    var firstPeriod = schedule.Min(x => x.Period);
                    var firstInstallment = schedule.First(x => x.Period == firstPeriod);

                    if (firstInstallment.Status == InstallmentStatus.Paid &&
                        order.Status == OrderStatus.Pending)
                    {
                        order.Status = OrderStatus.Confirmed;
                    }

                    // 2) Kiểm tra tất cả các kỳ đã được thanh toán hay chưa
                    var allPaid = schedule.All(x => x.Status == InstallmentStatus.Paid);

                    // Nếu tất cả các kỳ đã được thanh toán và đơn hàng đang ở trạng thái Installing
                    // (đã hoàn thành quy trình giao/nhận hàng) thì chuyển sang Completed.
                    if (allPaid && order.Status == OrderStatus.Installing)
                    {
                        order.Status = OrderStatus.Completed;
                    }
                }

                return;
            }

            // ===== FULL PAYMENT FLOW =====
            // Đơn hàng trả thẳng: Khi thanh toán thành công lần đầu, từ Pending → Confirmed
            var orderPayments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            var anySuccessPayment = orderPayments.Any(p => p.Status == PaymentStatus.Success);

            if (anySuccessPayment && order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Confirmed;
            }

            // Các bước Processing / Shipping / ReadyForPickup / Delivered / PickedUp / Completed
            // sẽ do workflow của staff ở OrderService/OrderController xử lý.
        }

        private async Task UpdateInstallmentPaidStatusBySumAsync(Guid installmentId, CancellationToken ct)
        {
            var ins = await _unitOfWork.InstallmentRepository.FindByIdWithTrackingAsync(installmentId)
                      ?? throw new NotFoundException("Không tìm thấy kỳ trả góp.");

            var payments = await _unitOfWork.PaymentRepository.GetByInstallmentIdAsync(installmentId);

            var successSum = payments
                .Where(p => p.Status == PaymentStatus.Success)
                .Sum(p => p.Amount);

            // Nếu enum InstallmentStatus của bạn có Paid/Pending thì dùng như này
            if (successSum >= ins.Amount)
                ins.Status = InstallmentStatus.Paid;
            else
                ins.Status = InstallmentStatus.Pending;
        }

        /// <summary>
        /// Đánh dấu tất cả các kỳ còn lại (Pending) của đơn hàng trả góp là Paid (cho tất toán).
        /// </summary>
        private async Task MarkAllRemainingInstallmentsAsPaidAsync(Guid orderId, CancellationToken ct)
        {
            var installments = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
            var remainingInstallments = installments
                .Where(i => i.Status == InstallmentStatus.Pending)
                .ToList();

            foreach (var installment in remainingInstallments)
            {
                var trackingInstallment =
                    await _unitOfWork.InstallmentRepository.FindByIdWithTrackingAsync(installment.Id)
                    ?? throw new NotFoundException("Không tìm thấy kỳ trả góp.");

                trackingInstallment.Status = InstallmentStatus.Paid;
            }
        }

        private static int ToVndInt(decimal amount)
        {
            if (amount <= 0) throw new BadRequestException("Số tiền phải lớn hơn 0.");
            if (amount != decimal.Truncate(amount)) throw new BadRequestException("Số tiền phải là số nguyên (VND).");
            if (amount > int.MaxValue) throw new BadRequestException("Số tiền quá lớn.");
            return checked((int)amount);
        }

        private static long CreateOrderCode()
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private async Task ClearSessionAsync(PayOsInitSession session)
        {
            var prefix = string.IsNullOrWhiteSpace(_payOs.RedisKeyPrefix) ? PayOsSessionPrefix : _payOs.RedisKeyPrefix;

            await _redisUtils.RemoveAsync($"{prefix}{session.SessionId}");
            await _redisUtils.RemoveAsync($"{PayOsIndexOrderCode}{session.OrderCode}");
            await _redisUtils.RemoveAsync($"{PayOsIndexOrder}{session.OrderId}");

            if (session.InstallmentId.HasValue)
                await _redisUtils.RemoveAsync($"{PayOsIndexInstallment}{session.InstallmentId.Value}");

            if (!string.IsNullOrWhiteSpace(session.PaymentLinkId))
                await _redisUtils.RemoveAsync($"{PayOsIndexPayLink}{session.PaymentLinkId}");
        }

        private static bool IsPayOsLinkStillValid(PayOsInitSession s, int bufferSeconds = 10)
        {
            if (s == null) return false;
            if (s.ExpiredAt <= 0) return false;
            if (string.IsNullOrWhiteSpace(s.RedirectUrl)) return false;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now < (s.ExpiredAt - bufferSeconds);
        }

        private async Task<PayOsInitSession?> TryGetReusableSessionByOrderIdAsync(Guid orderId)
        {
            var sidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexOrder}{orderId}");
            if (string.IsNullOrWhiteSpace(sidStr) || !Guid.TryParse(sidStr, out var sid))
                return null;

            var session = await GetSessionByIdAsync(sid);
            if (session == null) return null;

            // Nếu link còn hạn => reuse
            if (IsPayOsLinkStillValid(session))
                return session;

            // Nếu link đã hết hạn nhưng session còn => clear để init tạo link mới
            await ClearSessionAsync(session);
            return null;
        }

        private async Task<PayOsInitSession?> TryGetReusableSessionByInstallmentIdAsync(Guid installmentId)
        {
            var sidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexInstallment}{installmentId}");
            if (string.IsNullOrWhiteSpace(sidStr) || !Guid.TryParse(sidStr, out var sid))
                return null;

            var session = await GetSessionByIdAsync(sid);
            if (session == null) return null;

            if (IsPayOsLinkStillValid(session))
                return session;

            await ClearSessionAsync(session);
            return null;
        }
    }



    public class PayOsInitSession
    {
        public Guid SessionId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? InstallmentId { get; set; }
        public PaidType PaidType { get; set; }
        public int AmountVnd { get; set; }
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }

        public string? RedirectUrl { get; set; }
        public long ExpiredAt { get; set; }

        /// <summary>
        /// Đánh dấu đây là thanh toán tất toán (full settlement) cho đơn hàng trả góp.
        /// </summary>
        public bool IsFullSettlement { get; set; } = false;
    }

    public class OnlinePaymentInitResult
    {
        public Guid SessionId { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public long ExpiredAt { get; set; }
    }

    public class GatewayCallbackResult
    {
        public bool Ok { get; set; }
        public string? Message { get; set; }
        public Payment? Payment { get; set; }
    }

    public class CancelOrderRefundResult
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal RefundAmount { get; set; }
        public string? PayoutId { get; set; }
        public string? Reason { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    
    
}
