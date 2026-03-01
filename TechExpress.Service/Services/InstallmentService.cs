using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TechExpress.Repository;
using TechExpress.Repository.CustomExceptions;
using TechExpress.Repository.Enums;
using TechExpress.Repository.Models;

namespace TechExpress.Service.Services
{
    public class InstallmentService
    {
        private readonly UnitOfWork _unitOfWork;

        public InstallmentService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Installment>> HandleCreateInstallmentScheduleAsync(
            Guid orderId,
            int months,
            CancellationToken ct = default)
        {
            if (months < 1 || months > 60)
                throw new BadRequestException("Số tháng trả góp phải trong khoảng 1..60.");

            // Order tracking để update PaidType + Duration
            var order = await _unitOfWork.OrderRepository.FindByIdWithTrackingAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            if (order.TotalPrice <= 0)
                throw new BadRequestException("Tổng tiền đơn hàng không hợp lệ.");

            // Nếu đã có schedule thì không tạo lại (tránh trùng)
            var existing = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
            if (existing != null && existing.Count > 0)
            {
                // Nếu bạn muốn idempotent: return existing;
                throw new BadRequestException("Đơn hàng đã có lịch trả góp.");
            }

            // Set intent cho order
            order.PaidType = PaidType.Pending;
            order.InstallmentDurationMonth = months;

            // ===== Chia tiền: chia đều + dồn dư vào kỳ cuối =====
            // VND thường là số nguyên; nếu TotalPrice có lẻ thì vẫn chia theo decimal
            var total = order.TotalPrice;
            var baseAmount = decimal.Floor(total / months);
            var remainder = total - (baseAmount * months);

            // DueDate: mỗi tháng kể từ OrderDate (hoặc Now)
            var start = order.OrderDate;

            var installments = new List<Installment>(months);

            for (int period = 1; period <= months; period++)
            {
                var amount = baseAmount;

                // dồn phần dư vào kỳ cuối để tổng = total
                if (period == months)
                    amount += remainder;

                // Nếu bạn muốn làm tròn (VND), có thể Round về 0 decimals:
                // amount = decimal.Round(amount, 0, MidpointRounding.AwayFromZero);

                var dueDate = start.AddMonths(period);

                installments.Add(new Installment
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Period = period,
                    Amount = amount,
                    Status = InstallmentStatus.Pending,
                    DueDate = dueDate
                });
            }

            await _unitOfWork.InstallmentRepository.AddRangeAsync(installments);
            await _unitOfWork.SaveChangesAsync();

            // trả về schedule vừa tạo (no tracking list ok)
            var created = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
            return created;
        }

        public async Task<List<Installment>> HandleGetInstallmentScheduleByOrderAsync(
            Guid orderId,
            CancellationToken ct = default)
        {
            // check order exists (optional nhưng nên có để trả 404 đúng)
            var order = await _unitOfWork.OrderRepository.FindByIdAsync(orderId)
                        ?? throw new NotFoundException("Không tìm thấy đơn hàng.");

            var schedule = await _unitOfWork.InstallmentRepository.GetByOrderIdAsync(orderId);
            return schedule;
        }
    }
}
