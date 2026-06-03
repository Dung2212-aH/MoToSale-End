using MoToSale.Common;
using MoToSale.DTO.Payments;
using MoToSale.Entities.Operations;
using MoToSale.Entities.Ordering;
using MoToSale.Entities.Payments;
using MoToSale.Repository;
using MoToSale.Repository.Inventory;
using MoToSale.Repository.Ordering;
using MoToSale.Repository.Payments;

namespace MoToSale.Services.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _payments;
    private readonly IOrderRepository _orders;
    private readonly IReservationRepository _reservations;
    private readonly AppDbContext _db;

    public PaymentService(IPaymentRepository payments, IOrderRepository orders, IReservationRepository reservations, AppDbContext db)
    {
        _payments = payments;
        _orders = orders;
        _reservations = reservations;
        _db = db;
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

        // Ghi thu quỹ: dòng tiền vào khi thu tiền khách (đồng bộ sổ quỹ với phiếu thu).
        _db.CashTransactions.Add(new CashTransaction
        {
            Code = $"CT{now:yyyyMMddHHmmssfff}", TransactionType = "Receipt", Category = "CustomerPayment",
            Amount = req.Amount, Method = req.Method, ReferenceType = "Payment", ReferenceId = order.Id,
            Note = $"Thu tiền đơn {order.Code} ({req.PaymentType})", RecordedBy = userId, OccurredAt = now, CreatedDate = now,
        });

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

        // Đã thu đủ + hàng đã giao (bán đứt tại quầy) → hoàn tất đơn.
        if (reachedFull
            && order.FulfillmentStatus == Common.FulfillmentStatus.Fulfilled
            && order.OrderStatus != OrderStatus.Completed
            && order.OrderStatus != OrderStatus.Cancelled)
        {
            var fromC = order.OrderStatus;
            order.OrderStatus = OrderStatus.Completed;
            _orders.AddStatusHistory(new OrderStatusHistory
            {
                OrderId = order.Id, FromStatus = fromC, ToStatus = OrderStatus.Completed,
                Note = "Hoàn tất (đã giao & thu đủ)", ChangedBy = userId, CreatedDate = now,
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

        var now = DateTime.UtcNow;
        payment.PaymentRecordStatus = Common.PaymentRecordStatus.Cancelled;
        payment.Note = string.IsNullOrWhiteSpace(reason) ? payment.Note : $"{payment.Note} | Hủy: {reason}";
        payment.UpdatedDate = now;
        _payments.Update(payment);

        // Đảo quỹ: dòng tiền ra để bù lại khoản thu đã hủy.
        _db.CashTransactions.Add(new CashTransaction
        {
            Code = $"CT{now:yyyyMMddHHmmssfff}", TransactionType = "Payment", Category = "PaymentReversal",
            Amount = payment.Amount, Method = payment.Method, ReferenceType = "Payment", ReferenceId = payment.OrderId,
            Note = $"Đảo phiếu thu {payment.Code}", OccurredAt = now, CreatedDate = now,
        });

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
}
