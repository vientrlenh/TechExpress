using System;
using System.Collections.Generic;
using TechExpress.Repository.Enums;

namespace TechExpress.Application.Dtos.Responses;

/// <summary>
/// Response: kết quả set intent trả thẳng.
/// </summary>
public class SetPaymentIntentResponse
{
    /// <summary>Order id.</summary>
    public Guid OrderId { get; set; }

    /// <summary>Loại trả: Full.</summary>
    public PaidType PaidType { get; set; }

    /// <summary>Phương thức thanh toán user đã chọn.</summary>
    public PaymentMethod Method { get; set; }
}

/// <summary>
/// Một kỳ trong lịch trả góp.
/// </summary>
public class InstallmentItemResponse
{
    public Guid Id { get; set; }
    public int Period { get; set; }
    public decimal Amount { get; set; }
    public InstallmentStatus Status { get; set; }
    public DateTimeOffset DueDate { get; set; }
}

/// <summary>
/// Response: kết quả tạo schedule trả góp.
/// </summary>
public class SetInstallmentIntentResponse
{
    public Guid OrderId { get; set; }
    public PaidType PaidType { get; set; }
    public int Months { get; set; }
    public List<InstallmentItemResponse> Schedule { get; set; } = new();
}

/// <summary>
/// Response: init online payment.
/// </summary>
public class InitOnlinePaymentResponse
{
    /// <summary>SessionId (Redis) để callback tra cứu.</summary>
    public Guid SessionId { get; set; }

    /// <summary>URL chuyển hướng sang cổng thanh toán (PayOS/VnPay).</summary>
    public string RedirectUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response: callback đã được xử lý hay chưa.
/// </summary>
public class GatewayCallbackResponse
{
    public bool Ok { get; set; }
}

/// <summary>
/// Response: staff ghi nhận thu tiền mặt.
/// </summary>
public class CashPaymentResponse
{
    public long PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
}

/// <summary>
/// Payment item (query).
/// </summary>
public class PaymentResponse
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? InstallmentId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
}

/// <summary>
/// Installment item (query).
/// </summary>
//public class InstallmentResponse
//{
//    public Guid Id { get; set; }
//    public Guid OrderId { get; set; }
//    public int Period { get; set; }
//    public decimal Amount { get; set; }
//    public InstallmentStatus Status { get; set; }
//    public DateTimeOffset DueDate { get; set; }
//}

/// <summary>
/// Response: refund.
/// </summary>
public class RefundPaymentResponse
{
    public bool Ok { get; set; }
    public long PaymentId { get; set; }
    public string? Reason { get; set; }
}
