using MoToSale.Common;
using MoToSale.DTO.Payments;
using MoToSale.Entities.Ordering;
using MoToSale.Entities.Payments;
using MoToSale.Repository.Inventory;
using MoToSale.Repository.Ordering;
using MoToSale.Repository.Payments;

namespace MoToSale.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _payments;
    private readonly IOrderRepository _orders;
    private readonly IReservationRepository _reservations;

    public PaymentService(IPaymentRepository payments, IOrderRepository orders, IReservationRepository reservations)
    {
        _payments = payments;
        _orders = orders;
        _reservations = reservations;
    }

    public async Task<int> RecordPaymentAsync(CreatePaymentRequest req, int? userId)
    {
        if (req.Amount <= 0) throw new PaymentException("Số tiền phải lớn hơn 0.");
        var order = await _orders.GetByIdAsync(req.OrderId) ?? throw new PaymentException("Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new PaymentException("Đơn đã hủy.");

        var now = DateTime.UtcNow;
        var paymentStatusBefore = order.PaymentStatus;
        var payment = new Payment
        {
            Code = $"TT{now:yyyyMMddHHmmssfff}",
            OrderId = order.Id,
            PaymentType = req.PaymentType,
            Amount = req.Amount,
            Method = req.Method,
            PaymentRecordStatus = PaymentRecordStatus.Paid, // ghi nhận thủ công = đã thu
            TransactionRef = req.TransactionRef,
            Note = req.Note,
            RecordedBy = userId,
            PaidAt = now,
            CreatedDate = now,
        };
        _payments.Add(payment);

        // Tổng đã thu (gồm phiếu vừa ghi).
        var totalPaid = await _payments.GetTotalPaidAsync(order.Id) + req.Amount;
        if (totalPaid > order.GrandTotal) throw new PaymentException("Tổng thanh toán vượt quá giá trị đơn.");

        order.RemainingAmount = Math.Max(0, order.GrandTotal - totalPaid);

        var reachedFull = totalPaid >= order.GrandTotal;
        var reachedDeposit = order.OrderType == Common.OrderType.Deposit && totalPaid >= order.DepositAmount;

        order.PaymentStatus = reachedFull
            ? Common.PaymentStatus.Paid
            : reachedDeposit ? Common.PaymentStatus.DepositPaid : Common.PaymentStatus.PartiallyPaid;
        if (order.PaymentStatus != paymentStatusBefore)
        {
            _orders.AddStatusHistory(new OrderStatusHistory
            {
                OrderId = order.Id, FromStatus = paymentStatusBefore, ToStatus = order.PaymentStatus,
                Note = "PaymentStatus: Recorded manual payment", ChangedBy = userId, CreatedDate = now,
            });
        }

        // Đủ điều kiện xác nhận → confirm giữ chỗ + chuyển đơn sang Confirmed (sẵn sàng phân phối).
        if ((reachedFull || reachedDeposit) && order.OrderStatus == OrderStatus.AwaitingPayment)
        {
            foreach (var r in await _reservations.GetByOrderAsync(order.Id))
            {
                if (r.ReservationStatus == ReservationStatus.Active)
                {
                    r.ReservationStatus = ReservationStatus.Confirmed;
                    r.UpdatedDate = now;
                }
            }

            var from = order.OrderStatus;
            order.OrderStatus = OrderStatus.Confirmed;
            _orders.AddStatusHistory(new OrderStatusHistory
            {
                OrderId = order.Id, FromStatus = from, ToStatus = OrderStatus.Confirmed,
                Note = "Xác nhận thanh toán", ChangedBy = userId, CreatedDate = now,
            });
        }

        order.UpdatedDate = now;
        _orders.Update(order);
        await _payments.SaveChangesAsync();
        return payment.Id;
    }

    public Task<MoToSale.DTO.Common.PagingResponse<PaymentListItem>> SearchAsync(MoToSale.DTO.Common.PagingRequest request, string? status) => _payments.SearchAsync(request, status);

    public async Task CancelAsync(int id, string? reason)
    {
        var payment = await _payments.GetByIdAsync(id) ?? throw new PaymentException("Không tìm thấy phiếu thanh toán.");
        if (payment.PaymentRecordStatus == Common.PaymentRecordStatus.Cancelled) throw new PaymentException("Phiếu đã hủy.");

        payment.PaymentRecordStatus = Common.PaymentRecordStatus.Cancelled;
        payment.Note = string.IsNullOrWhiteSpace(reason) ? payment.Note : $"{payment.Note} | Hủy: {reason}";
        payment.UpdatedDate = DateTime.UtcNow;
        _payments.Update(payment);

        // Tính lại trạng thái thanh toán đơn.
        var order = await _orders.GetByIdAsync(payment.OrderId);
        if (order is not null)
        {
            var paymentStatusBefore = order.PaymentStatus;
            var totalPaid = await _payments.GetTotalPaidAsync(order.Id) - payment.Amount;
            order.RemainingAmount = Math.Max(0, order.GrandTotal - totalPaid);
            order.PaymentStatus = totalPaid <= 0 ? Common.PaymentStatus.Unpaid
                : totalPaid >= order.GrandTotal ? Common.PaymentStatus.Paid
                : order.OrderType == Common.OrderType.Deposit && totalPaid >= order.DepositAmount ? Common.PaymentStatus.DepositPaid
                : Common.PaymentStatus.PartiallyPaid;
            order.UpdatedDate = DateTime.UtcNow;
            _orders.Update(order);
            if (order.PaymentStatus != paymentStatusBefore)
            {
                _orders.AddStatusHistory(new OrderStatusHistory
                {
                    OrderId = order.Id, FromStatus = paymentStatusBefore, ToStatus = order.PaymentStatus,
                    Note = "PaymentStatus: Cancelled manual payment", CreatedDate = DateTime.UtcNow,
                });
            }
        }
        await _payments.SaveChangesAsync();
    }

    public async Task<List<PaymentDto>> GetByOrderAsync(int orderId)
    {
        var list = await _payments.GetByOrderAsync(orderId);
        return list.Select(p => new PaymentDto(
            p.Id, p.Code, p.OrderId, p.PaymentType, p.Amount, p.Method, p.PaymentRecordStatus, p.TransactionRef, p.PaidAt)).ToList();
    }

    // ===== Customer-facing =====

    /// <summary>
    /// Khách hàng khởi tạo phiếu thanh toán ở trạng thái Pending (chờ admin xác nhận đã nhận tiền).
    /// Không tự động chuyển đơn sang đã thanh toán.
    /// </summary>
    public async Task<int> CreateCustomerPaymentAsync(CreateCustomerPaymentRequest req, int userId)
    {
        var order = await _orders.GetByIdAsync(req.OrderId) ?? throw new PaymentException("Không tìm thấy đơn hàng.");
        if (order.UserId != userId) throw new PaymentException("Bạn không có quyền thanh toán cho đơn hàng này.");
        if (order.OrderStatus == OrderStatus.Cancelled) throw new PaymentException("Đơn đã hủy.");

        // Số tiền: dùng giá trị gửi lên nếu hợp lệ, ngược lại lấy số còn phải trả của đơn.
        var totalPaid = await _payments.GetTotalPaidAsync(order.Id);
        var remaining = Math.Max(0, order.GrandTotal - totalPaid);
        var amount = req.Amount is > 0 ? req.Amount.Value : remaining;
        if (amount <= 0) throw new PaymentException("Đơn hàng đã được thanh toán đủ.");
        if (amount > remaining) throw new PaymentException("Số tiền vượt quá số còn phải thanh toán.");

        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            Code = $"TT{now:yyyyMMddHHmmssfff}",
            OrderId = order.Id,
            PaymentType = string.IsNullOrWhiteSpace(req.PaymentType) ? PaymentRecordType.Full : req.PaymentType,
            Amount = amount,
            Method = string.IsNullOrWhiteSpace(req.Method) ? PaymentMethod.BankTransfer : req.Method,
            PaymentRecordStatus = PaymentRecordStatus.Pending, // chờ admin xác nhận
            TransactionRef = req.TransactionRef,
            Note = req.Note,
            RecordedBy = userId,
            PaidAt = null,
            CreatedDate = now,
        };
        _payments.Add(payment);
        await _payments.SaveChangesAsync();
        return payment.Id;
    }

    /// <summary>
    /// Khách hàng báo "đã chuyển khoản". Phiếu vẫn ở trạng thái Pending (chờ admin xác nhận thực thu),
    /// chỉ lưu mã giao dịch / ghi chú để admin đối soát. KHÔNG tự động chuyển sang Paid.
    /// </summary>
    public async Task ConfirmSuccessAsync(int paymentId, int? userId)
    {
        var payment = await _payments.GetByIdAsync(paymentId) ?? throw new PaymentException("Không tìm thấy phiếu thanh toán.");
        if (payment.PaymentRecordStatus == PaymentRecordStatus.Cancelled) throw new PaymentException("Phiếu đã hủy.");
        if (payment.PaymentRecordStatus == PaymentRecordStatus.Paid) throw new PaymentException("Phiếu đã được xác nhận thanh toán.");

        var now = DateTime.UtcNow;
        var note = "Khách báo đã chuyển khoản, chờ xác nhận.";
        payment.Note = string.IsNullOrWhiteSpace(payment.Note) ? note : $"{payment.Note} | {note}";
        payment.UpdatedDate = now;
        _payments.Update(payment);
        await _payments.SaveChangesAsync();
    }

    public async Task<int?> GetOrderOwnerAsync(int orderId)
    {
        var order = await _orders.GetByIdAsync(orderId);
        return order?.UserId;
    }

    public async Task<int?> GetPaymentOwnerAsync(int paymentId)
    {
        var payment = await _payments.GetByIdAsync(paymentId);
        if (payment is null) return null;
        var order = await _orders.GetByIdAsync(payment.OrderId);
        return order?.UserId;
    }
}
