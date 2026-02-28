using System;
using TechExpress.Repository.Enums;

namespace TechExpress.Repository.Models;

public class Payment
{
    public long Id { get; set; }

    public required Guid OrderId { get; set; }

    public Guid? InstallmentId { get; set; }

    public required decimal Amount { get; set; }

    public required PaymentMethod Method { get; set; }

    public required PaymentStatus Status { get; set; }

    public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.Now;

    public Order Order { get; set; } = null!;

    public Installment? Installment { get; set; }
}
