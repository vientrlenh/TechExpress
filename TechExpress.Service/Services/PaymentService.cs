using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            var items = new List<ItemData>
    {
        new ItemData("Thanh toán đơn hàng", 1, amount)
    };

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: amount, 
                description: "Thanh toán đơn",
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
                AmountVnd = amount,
                OrderCode = orderCode,
                PaymentLinkId = response.paymentLinkId,
                RedirectUrl = response.checkoutUrl,
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

            var items = new List<ItemData>
    {
        new ItemData($"Thanh toán trả góp kỳ {installment.Period}", 1, amount)
    };

            var finalReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? _payOs.ReturnUrl : returnUrl;

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: amount,
                description: $"Trả góp kỳ {installment.Period}",
                items: items,
                returnUrl: finalReturnUrl,
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
                RedirectUrl = response.checkoutUrl,
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
        // =========================
        // 3) GATEWAY CALLBACK (Controller đang gọi method này)
        // =========================
        public async Task<GatewayCallbackResult> HandlePayOsWebhookAsync(
             PayOsWebhookRequest req,
             CancellationToken ct = default)
        {
            if (req == null) throw new BadRequestException("Callback payload không hợp lệ.");

            // (tạm thời) chỉ check có signature, chưa verify
            if (string.IsNullOrWhiteSpace(req.Signature))
                return new GatewayCallbackResult { Ok = false, Message = "Thiếu signature." };

            // 1) resolve session
            var session = await ResolvePayOsSessionAsync(req);
            if (session == null)
                return new GatewayCallbackResult { Ok = false, Message = "Session không tồn tại hoặc đã hết hạn." };

            // 2) idempotency lock (theo orderCode là hợp lý)
            var lockKey = $"{PayOsCallbackLock}{session.OrderCode}";
            var locked = await _redisUtils.TrySetStringIfNotExists(lockKey, "1", TimeSpan.FromDays(2));
            if (!locked)
                return new GatewayCallbackResult { Ok = true, Message = "Duplicate webhook (ignored)." };

            // 3) status + amount
            var status = req.Success ? PaymentStatus.Success : PaymentStatus.Failed;
            var paidAmount = req.Data.Amount > 0 ? (decimal)req.Data.Amount : session.AmountVnd;

            // 4) insert payment
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

            // 5) update statuses nếu success (giữ nguyên cách bạn đang làm)
            if (status == PaymentStatus.Success)
            {
                if (session.InstallmentId.HasValue)
                {
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);
                    await _unitOfWork.SaveChangesAsync();
                }

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

        private async Task<PayOsInitSession?> ResolvePayOsSessionAsync(PayOsWebhookRequest req)
        {
            // ưu tiên paymentLinkId
            if (!string.IsNullOrWhiteSpace(req.Data?.PaymentLinkId))
            {
                var sidStr = await _redisUtils.GetStringDataFromKey($"{PayOsIndexPayLink}{req.Data.PaymentLinkId}");
                if (Guid.TryParse(sidStr, out var sid))
                    return await GetSessionByIdAsync(sid);
            }

            // fallback orderCode
            if (req.Data?.OrderCode > 0)
                return await FindSessionByOrderCodeAsync(req.Data.OrderCode);

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
                if (session.InstallmentId.HasValue)
                    await UpdateInstallmentPaidStatusBySumAsync(session.InstallmentId.Value, ct);

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
    /// <summary>
    /// Payload webhook của PayOS.
    /// Top-level: code, desc, success, data, signature.
    /// </summary>
    public sealed class PayOsWebhookRequest
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public PayOsWebhookData Data { get; set; } = new();

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    public sealed class PayOsWebhookData
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("transactionDateTime")]
        public string TransactionDateTime { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("paymentLinkId")]
        public string PaymentLinkId { get; set; } = string.Empty;

        // code/desc nằm trong data (theo docs)
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        // optional fields
        [JsonPropertyName("counterAccountBankId")]
        public string? CounterAccountBankId { get; set; }

        [JsonPropertyName("counterAccountBankName")]
        public string? CounterAccountBankName { get; set; }

        [JsonPropertyName("counterAccountName")]
        public string? CounterAccountName { get; set; }

        [JsonPropertyName("counterAccountNumber")]
        public string? CounterAccountNumber { get; set; }

        [JsonPropertyName("virtualAccountName")]
        public string? VirtualAccountName { get; set; }

        [JsonPropertyName("virtualAccountNumber")]
        public string? VirtualAccountNumber { get; set; }
    }
}
