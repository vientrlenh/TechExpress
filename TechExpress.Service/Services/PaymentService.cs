using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;
using TechExpress.Service.Utils;
using TechExpress.Service.Utils.TechExpress.Service.Utils;

namespace TechExpress.Service.Services
{
    public class PaymentService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly RedisUtils _redisUtils;
        private readonly PayOsClient _payOs;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        // ===== Redis Keys =====
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
    CancellationToken ct = default)
        {
            if (method != PaymentMethod.PayOs)
                throw new BadRequestException("Bản hiện tại chỉ implement PayOS.");

            // ✅ 0) idempotent: nếu có session active thì trả lại
            var existingSidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexOrder}{orderId}");
            if (!string.IsNullOrWhiteSpace(existingSidStr) && Guid.TryParse(existingSidStr, out var existingSid))
            {
                var existingSession = await GetSessionByIdAsync(existingSid);
                if (existingSession != null && !string.IsNullOrWhiteSpace(existingSession.RedirectUrl))
                {
                    return new OnlinePaymentInitResult
                    {
                        SessionId = existingSession.SessionId,
                        RedirectUrl = existingSession.RedirectUrl!,
                        OrderCode = existingSession.OrderCode,
                        PaymentLinkId = existingSession.PaymentLinkId,
                        ExpiredAt = 0 // (nếu muốn chuẩn thì thêm field ExpiredAt vào session)
                    };
                }
            }

            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.TotalPrice <= 0)
                throw new BadRequestException("Tổng tiền đơn hàng không hợp lệ.");

            var amount = ToVndInt(order.TotalPrice);

            var orderCode = CreateOrderCode();
            var expiredAt = DateTimeOffset.UtcNow.AddSeconds(_payOs.ExpirationSeconds).ToUnixTimeSeconds();

            var items = new List<ItemData>
    {
        new ItemData("Thanh toán đơn hàng", 1, amount)
    };

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: 10000, // bạn đang test
                description: $"Thanh toán đơn",
                items: items,
                returnUrl: finalReturnUrl,
                cancelUrl: _payOs.CancelUrl,
                expiredAt: expiredAt
            );

            var response = await _payOs.CreatePaymentLinkAsync(paymentData);

            var session = new PayOsInitSession
            {
                SessionId = Guid.NewGuid(),
                OrderId = orderId,
                InstallmentId = null,
                PaidType = PaidType.Full,
                AmountVnd = 10000,
                OrderCode = orderCode,
                PaymentLinkId = response.paymentLinkId,
                RedirectUrl = response.checkoutUrl // ✅ lưu để trả lại lần sau
            };

            await SaveSessionAsync(session);

            return new OnlinePaymentInitResult
            {
                SessionId = session.SessionId,
                RedirectUrl = response.checkoutUrl,
                OrderCode = orderCode,
                PaymentLinkId = response.paymentLinkId,
                ExpiredAt = expiredAt
            };
        }
        public async Task<OnlinePaymentInitResult> HandleInitInstallmentOnlinePaymentAsync(
    Guid installmentId,
    PaymentMethod method,
    CancellationToken ct = default)
        {
            if (method != PaymentMethod.PayOs)
                throw new BadRequestException("Bản hiện tại chỉ implement PayOS.");

            // ✅ 0) idempotent: nếu có session active thì trả lại
            var existingSidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexInstallment}{installmentId}");
            if (!string.IsNullOrWhiteSpace(existingSidStr) && Guid.TryParse(existingSidStr, out var existingSid))
            {
                var existingSession = await GetSessionByIdAsync(existingSid);
                if (existingSession != null && !string.IsNullOrWhiteSpace(existingSession.RedirectUrl))
                {
                    return new OnlinePaymentInitResult
                    {
                        SessionId = existingSession.SessionId,
                        RedirectUrl = existingSession.RedirectUrl!,
                        OrderCode = existingSession.OrderCode,
                        PaymentLinkId = existingSession.PaymentLinkId,
                        ExpiredAt = 0
                    };
                }
            }

            var installment = await _unitOfWork.InstallmentRepository.FindByIdAsync(installmentId)
                ?? throw new NotFoundException("Không tìm thấy kỳ trả góp.");

            var amount = ToVndInt(installment.Amount);

            var orderCode = CreateOrderCode();
            var expiredAt = DateTimeOffset.UtcNow.AddSeconds(_payOs.ExpirationSeconds).ToUnixTimeSeconds();

            var items = new List<ItemData>
    {
        new ItemData($"Thanh toán trả góp kỳ {installment.Period}", 1, amount)
    };

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: amount,
                description: $"Trả góp kỳ {installment.Period}",
                items: items,
                returnUrl: _payOs.ReturnUrl,
                cancelUrl: _payOs.CancelUrl,
                expiredAt: expiredAt
            );

            var response = await _payOs.CreatePaymentLinkAsync(paymentData);

            var session = new PayOsInitSession
            {
                SessionId = Guid.NewGuid(),
                OrderId = installment.OrderId,
                InstallmentId = installmentId,
                PaidType = PaidType.Installment,
                AmountVnd = amount,
                OrderCode = orderCode,
                PaymentLinkId = response.paymentLinkId,
                RedirectUrl = response.checkoutUrl // ✅ lưu để trả lại lần sau
            };

            await SaveSessionAsync(session);

            return new OnlinePaymentInitResult
            {
                SessionId = session.SessionId,
                RedirectUrl = response.checkoutUrl,
                OrderCode = orderCode,
                PaymentLinkId = response.paymentLinkId,
                ExpiredAt = expiredAt
            };
        }
        // =========================
        // 3) GATEWAY CALLBACK (Controller đang gọi method này)
        // =========================
        public async Task<GatewayCallbackResult> HandleGatewayCallbackAsync(
    string provider,
    object callbackPayload,
    CancellationToken ct = default)
        {
            provider = (provider ?? "").Trim().ToLowerInvariant();

            if (provider != "payos")
                throw new BadRequestException("Provider không được hỗ trợ (bản này chỉ demo PayOS).");

            if (callbackPayload is not GatewayCallbackRequest req)
                throw new BadRequestException("Callback payload không hợp lệ.");

            var session = await GetSessionByIdAsync(req.SessionId);
            if (session == null)
                return new GatewayCallbackResult { Ok = false, Message = "Session không tồn tại hoặc đã hết hạn." };

            var lockKey = $"{PayOsCallbackLock}{session.SessionId}";
            var locked = await _redisUtils.TrySetStringIfNotExists(lockKey, "1", TimeSpan.FromDays(2));
            if (!locked)
                return new GatewayCallbackResult { Ok = true, Message = "Duplicate callback (ignored)." };

            if (string.IsNullOrWhiteSpace(req.Signature))
                return new GatewayCallbackResult { Ok = false, Message = "Thiếu chữ ký callback." };

            var status = req.Success ? PaymentStatus.Success : PaymentStatus.Failed;
            var paidAmount = req.PaidAmount > 0 ? req.PaidAmount : session.AmountVnd;

            var payment = new Payment
            {
                OrderId = session.OrderId,
                InstallmentId = session.InstallmentId,
                Amount = paidAmount,
                Method = PaymentMethod.PayOs,
                Status = status,
                PaymentDate = DateTimeOffset.Now
            };

            // 1-2) Add + flush payment
            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            if (status == PaymentStatus.Success)
            {
                // 3-4) Update installment + flush installment status (để Order.Completed đúng ngay)
                if (session.InstallmentId.HasValue)
                {
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);
                    await _unitOfWork.SaveChangesAsync();
                }

                // 5-6) Update order + save
                await UpdateOrderStatusAfterPaymentAsync(session.OrderId, ct);
                await _unitOfWork.SaveChangesAsync();
            }

            return new GatewayCallbackResult
            {
                Ok = true,
                Payment = payment,
                Message = "Callback processed."
            };
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

            // ✅ FIX: flush payment xuống DB trước khi Sum()
            await _unitOfWork.PaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();



            // 4) update order/installment khi success
            if (status == PaymentStatus.Success)
            {
                if (session.InstallmentId.HasValue)
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);

                await UpdateOrderStatusAfterPaymentAsync(session.OrderId, ct);

                // ✅ persist status changes
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
        // 6) REFUND
        // =========================
        public async Task<Payment> HandleRefundPaymentAsync(long paymentId, string? reason, CancellationToken ct = default)
        {
            var payment = await _unitOfWork.PaymentRepository.FindByIdAsync(paymentId)
                          ?? throw new NotFoundException("Không tìm thấy payment.");

            // WARNING: FindByIdAsync đang AsNoTracking trong repo bạn đưa
            // => đổi repo FindByIdWithTrackingAsync hoặc implement UpdateAsync(...) để update được.
            // Ở đây để chạy compile, mình sẽ throw rõ ràng:
            throw new BadRequestException("PaymentRepository.FindByIdAsync đang AsNoTracking. Hãy bổ sung method tracking/update để refund.");

            // payment.Status = PaymentStatus.Refunded;
            // await _unitOfWork.PaymentRepository.UpdateAsync(payment);
            // await _unitOfWork.SaveChangesAsync();
            // return payment;
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

        // Update Order.Status dựa trên tổng tiền đã trả thành công
        private async Task UpdateOrderStatusAfterPaymentAsync(Guid orderId, CancellationToken ct)
        {
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            // ===== INSTALLMENT FLOW =====
            if (order.PaidType == PaidType.Installment)
            {
                // 1) Có ít nhất 1 payment success => Processing
                var payments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
                var anySuccessPayment = payments.Any(p => p.Status == PaymentStatus.Success);

                if (anySuccessPayment && order.Status == OrderStatus.Pending)
                    order.Status = OrderStatus.Processing;

                // 2) Tất cả kỳ Paid => Success
                var schedule = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
                if (schedule != null && schedule.Count > 0)
                {
                    var allPaid = schedule.All(x => x.Status == InstallmentStatus.Paid);
                    if (allPaid)
                        order.Status = OrderStatus.Completed;
                }

                return;
            }

            // ===== FULL PAYMENT FLOW =====
            var orderPayments = await _unitOfWork.PaymentRepository.GetByOrderIdAsync(orderId);
            var successSum = orderPayments.Where(p => p.Status == PaymentStatus.Success).Sum(p => p.Amount);

            if (successSum >= order.TotalPrice)
            {
                // tùy nghiệp vụ: muốn paid đủ là Success thì set Success, nếu không thì Processing
                // order.Status = OrderStatus.Success;

                if (order.Status == OrderStatus.Pending)
                    order.Status = OrderStatus.Processing;
            }
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
    }



    // ==========================================================
    // DTOs (để 1 file copy-paste chạy luôn)
    // ==========================================================
    public class PayOsInitSession
    {
        public Guid SessionId { get; set; }
        public Guid OrderId { get; set; }
        public Guid? InstallmentId { get; set; }
        public PaidType PaidType { get; set; }
        public int AmountVnd { get; set; }
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }

        public string? RedirectUrl { get; set; } // ✅ thêm để init trả lại link cũ
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

    // Model request giống controller DTO demo (để compile service nếu bạn đặt chung project Service)
    public class GatewayCallbackRequest
    {
        public Guid SessionId { get; set; }
        public bool Success { get; set; }
        public decimal PaidAmount { get; set; }
        public string Signature { get; set; } = string.Empty;
        public string? Raw { get; set; }
    }
}
