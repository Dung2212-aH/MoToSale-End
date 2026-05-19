using System.Data;
using PaymentService.DTOs.Common;
using PaymentService.DTOs.Payments;
using PaymentService.Entities;
using PaymentService.Exceptions;
using PaymentService.Repositories;

namespace PaymentService.Services;

public class PaymentService : IPaymentService
{
    private const string ActiveUserStatus = "Active";
    private const string PendingPaymentStatus = "Pending";
    private const string PaidPaymentStatus = "Paid";
    private const string FailedPaymentStatus = "Failed";
    private const string CancelledPaymentStatus = "Cancelled";
    private const string CancelledOrderStatus = "Cancelled";
    private const string ConfirmedOrderStatus = "Confirmed";

    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "COD",
        "BankTransfer",
        "Card",
        "Momo",
        "VNPay"
    };

    private static readonly HashSet<string> AllowedPaymentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Full",
        "Deposit",
        "Remaining",
        "Installment"
    };

    private static readonly HashSet<string> SuccessfulPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PaidPaymentStatus
    };

    private static readonly HashSet<string> InitialPaymentOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Checkout",
        "AwaitingPayment"
    };

    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PagedResultDto<PaymentDto>> GetPaymentsAsync(
        PaymentSearchDto search,
        int currentUserId,
        bool canManagePayments)
    {
        await EnsureActiveUserAsync(currentUserId);

        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);
        int? userFilter = canManagePayments ? null : currentUserId;

        var payments = await _paymentRepository.GetPaymentsAsync(search, userFilter);
        var totalItems = await _paymentRepository.CountPaymentsAsync(search, userFilter);

        return new PagedResultDto<PaymentDto>
        {
            Items = payments.Select(MapPayment).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PaymentDto> GetPaymentByIdAsync(int maThanhToan, int currentUserId, bool canManagePayments)
    {
        await EnsureActiveUserAsync(currentUserId);
        var payment = await GetPaymentForUserAsync(maThanhToan, currentUserId, canManagePayments);
        return MapPayment(payment);
    }

    public async Task<PaymentOrderSummaryDto> GetOrderPaymentSummaryAsync(int maDonHang, int currentUserId, bool canManagePayments)
    {
        await EnsureActiveUserAsync(currentUserId);
        var order = await GetOrderForUserAsync(maDonHang, currentUserId, canManagePayments);
        return MapOrderPaymentSummary(order);
    }

    public async Task<PaymentDto> CreatePaymentAsync(int currentUserId, bool canManagePayments, CreatePaymentRequest request)
    {
        await EnsureActiveUserAsync(currentUserId);

        await using var transaction = await _paymentRepository.BeginTransactionAsync(IsolationLevel.Serializable);
        await _paymentRepository.CleanupExpiredInventoryHoldsAsync();

        var order = await GetOrderForUserAsync(request.MaDonHang, currentUserId, canManagePayments);
        EnsureOrderCanReceivePayment(order);

        var paymentType = NormalizeAllowedValue(request.LoaiThanhToan, AllowedPaymentTypes, "Loai thanh toan khong hop le.");
        var paymentMethod = NormalizeAllowedValue(request.PhuongThuc, AllowedPaymentMethods, "Phuong thuc thanh toan khong hop le.");
        ValidatePaymentRequest(order, paymentType, request.SoTien);

        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            MaThanhToanKinhDoanh = GeneratePaymentCode(),
            MaDonHang = order.MaDonHang,
            SoTien = request.SoTien,
            PhuongThuc = paymentMethod,
            TrangThai = PendingPaymentStatus,
            MaGiaoDich = TrimToNull(request.MaGiaoDich),
            DaThanhToanLuc = null,
            NgayTao = now,
            LoaiThanhToan = paymentType,
            NoiDungChuyenKhoan = TrimToNull(request.NoiDungChuyenKhoan),
            MaNganHang = TrimToNull(request.MaNganHang),
            ResponseRaw = TrimToNull(request.ResponseRaw),
            Order = order
        };

        await _paymentRepository.AddPaymentAsync(payment);
        await _paymentRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapPayment(payment);
    }

    public async Task<PaymentOrderSummaryDto> ConfirmPaymentSuccessAsync(
        int maThanhToan,
        int currentUserId,
        bool canManagePayments,
        ConfirmPaymentRequest request)
    {
        await EnsureActiveUserAsync(currentUserId);

        await using var transaction = await _paymentRepository.BeginTransactionAsync(IsolationLevel.Serializable);
        await _paymentRepository.CleanupExpiredInventoryHoldsAsync();

        var payment = await GetPaymentForUserAsync(maThanhToan, currentUserId, canManagePayments);
        if (payment.TrangThai != PendingPaymentStatus)
        {
            throw new BusinessException("Chi co the xac nhan giao dich Pending.");
        }

        var order = payment.Order ?? throw new NotFoundException("Khong tim thay don hang cua giao dich.");
        EnsureOrderCanReceivePayment(order);

        var now = DateTime.UtcNow;
        payment.TrangThai = PaidPaymentStatus;
        payment.MaGiaoDich = TrimToNull(request.MaGiaoDich) ?? payment.MaGiaoDich;
        payment.DaThanhToanLuc = now;
        payment.ResponseRaw = TrimToNull(request.ResponseRaw) ?? payment.ResponseRaw;

        SyncOrderPaymentStatus(order, now);

        var summary = CalculateSummary(order);
        if (ShouldConfirmOrder(order, summary.TongDaThanhToan))
        {
            await ConfirmOrderAndDeductStockAsync(order, now);
        }

        await _paymentRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapOrderPaymentSummary(order);
    }

    public async Task<PaymentDto> MarkPaymentFailedAsync(
        int maThanhToan,
        int currentUserId,
        bool canManagePayments,
        FailPaymentRequest request)
    {
        await EnsureActiveUserAsync(currentUserId);
        if (!canManagePayments)
        {
            throw new ForbiddenException("Ban khong co quyen cap nhat giao dich thanh toan.");
        }

        var payment = await GetPaymentForUserAsync(maThanhToan, currentUserId, canManagePayments);
        if (payment.TrangThai != PendingPaymentStatus)
        {
            throw new BusinessException("Chi co the danh dau that bai giao dich Pending.");
        }

        payment.TrangThai = FailedPaymentStatus;
        payment.LyDoHuy = TrimToNull(request.LyDo);
        payment.ResponseRaw = TrimToNull(request.ResponseRaw) ?? payment.ResponseRaw;

        if (payment.Order is not null)
        {
            SyncOrderPaymentStatus(payment.Order, DateTime.UtcNow);
        }

        await _paymentRepository.SaveChangesAsync();
        return MapPayment(payment);
    }

    public async Task<PaymentDto> CancelPaymentAsync(
        int maThanhToan,
        int currentUserId,
        bool canManagePayments,
        CancelPaymentRequest request)
    {
        await EnsureActiveUserAsync(currentUserId);

        var payment = await GetPaymentForUserAsync(maThanhToan, currentUserId, canManagePayments);
        if (payment.TrangThai is not PendingPaymentStatus and not FailedPaymentStatus)
        {
            throw new BusinessException("Chi co the huy giao dich Pending hoac Failed.");
        }

        payment.TrangThai = CancelledPaymentStatus;
        payment.LyDoHuy = TrimToNull(request.LyDoHuy);
        payment.NgayHuy = DateTime.UtcNow;

        if (payment.Order is not null)
        {
            SyncOrderPaymentStatus(payment.Order, DateTime.UtcNow);
        }

        await _paymentRepository.SaveChangesAsync();
        return MapPayment(payment);
    }

    private async Task EnsureActiveUserAsync(int maNguoiDung)
    {
        var user = await _paymentRepository.GetUserAsync(maNguoiDung)
            ?? throw new NotFoundException("Khong tim thay nguoi dung.");

        if (!string.Equals(user.TrangThai, ActiveUserStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Tai khoan khong o trang thai Active.");
        }
    }

    private async Task<Order> GetOrderForUserAsync(int maDonHang, int currentUserId, bool canManagePayments)
    {
        var order = await _paymentRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        if (!canManagePayments && order.MaNguoiDung != currentUserId)
        {
            throw new ForbiddenException("Ban khong co quyen truy cap don hang nay.");
        }

        return order;
    }

    private async Task<Payment> GetPaymentForUserAsync(int maThanhToan, int currentUserId, bool canManagePayments)
    {
        var payment = await _paymentRepository.GetPaymentByIdAsync(maThanhToan)
            ?? throw new NotFoundException("Khong tim thay giao dich thanh toan.");

        if (!canManagePayments && payment.Order?.MaNguoiDung != currentUserId)
        {
            throw new ForbiddenException("Ban khong co quyen truy cap giao dich thanh toan nay.");
        }

        return payment;
    }

    private static void EnsureOrderCanReceivePayment(Order order)
    {
        if (string.Equals(order.TrangThaiDonHang, CancelledOrderStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Don hang da huy, khong the thanh toan.");
        }
    }

    private static void ValidatePaymentRequest(Order order, string paymentType, decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessException("So tien thanh toan phai lon hon 0.");
        }

        var summary = CalculateSummary(order);
        if (summary.SoTienConPhaiThu <= 0)
        {
            throw new BusinessException("Don hang da thanh toan du.");
        }

        if (amount > summary.SoTienConPhaiThu)
        {
            throw new BusinessException("So tien thanh toan vuot qua so tien con phai thu.");
        }

        if (paymentType == "Full" && summary.TongDaThanhToan > 0)
        {
            throw new BusinessException("Don hang da co thanh toan truoc do, khong the tao thanh toan Full.");
        }

        if (paymentType == "Full" && amount != summary.SoTienConPhaiThu)
        {
            throw new BusinessException("Thanh toan Full phai bang dung so tien con phai thu.");
        }

        if (order.LoaiDonHang == "FullPayment" && paymentType != "Full")
        {
            throw new BusinessException("Don thanh toan toan bo chi chap nhan LoaiThanhToan Full.");
        }
    }

    private void SyncOrderPaymentStatus(Order order, DateTime now)
    {
        var summary = CalculateSummary(order);
        var total = order.TongThanhToan;
        var status = summary switch
        {
            _ when total <= 0 => "Paid",
            _ when summary.TongDaThanhToan >= total => "Paid",
            _ when summary.TongDaThanhToan > 0 => "PartiallyPaid",
            _ when order.Payments.Any(p => p.TrangThai == FailedPaymentStatus) &&
                   order.Payments.All(p => p.TrangThai is FailedPaymentStatus or CancelledPaymentStatus) => "Failed",
            _ => "Unpaid"
        };

        order.TrangThaiThanhToan = status;
        if (status == "Paid" && order.NgayThanhToanThanhCong is null)
        {
            order.NgayThanhToanThanhCong = now;
        }

        order.NgayCapNhat = now;
    }

    private static bool ShouldConfirmOrder(Order order, decimal totalNetPaid)
    {
        if (!InitialPaymentOrderStatuses.Contains(order.TrangThaiDonHang))
        {
            return false;
        }

        return order.LoaiDonHang switch
        {
            "FullPayment" => totalNetPaid >= order.TongThanhToan,
            "Deposit" => totalNetPaid >= order.TienDatCoc,
            "Installment" => order.TienDatCoc > 0 ? totalNetPaid >= order.TienDatCoc : totalNetPaid > 0,
            _ => false
        };
    }

    private async Task ConfirmOrderAndDeductStockAsync(Order order, DateTime now)
    {
        var activeHolds = order.InventoryHolds
            .Where(h => h.TrangThai == "Active" && h.HetHanLuc > now)
            .ToList();

        if (activeHolds.Count == 0)
        {
            throw new BusinessException("Don hang khong con giu cho ton kho hieu luc. Vui long checkout lai.");
        }

        foreach (var group in activeHolds.Where(h => h.MaBienSanPham.HasValue).GroupBy(h => h.MaBienSanPham!.Value))
        {
            var variant = await _paymentRepository.GetVariantAsync(group.Key)
                ?? throw new BusinessException("Bien the san pham trong don hang khong ton tai.");
            var requiredQuantity = group.Sum(h => h.SoLuong);
            var stock = variant.SoLuongTon ?? 0;

            if (stock < requiredQuantity)
            {
                throw new BusinessException("So luong ton kho bien the khong du de xac nhan don hang.");
            }

            variant.SoLuongTon = stock - requiredQuantity;
            variant.NgayCapNhat = now;
        }

        foreach (var group in activeHolds.Where(h => !h.MaBienSanPham.HasValue).GroupBy(h => h.MaSanPham))
        {
            var product = await _paymentRepository.GetProductAsync(group.Key)
                ?? throw new BusinessException("San pham trong don hang khong ton tai.");
            var requiredQuantity = group.Sum(h => h.SoLuong);

            if (product.SoLuongTon < requiredQuantity)
            {
                throw new BusinessException("So luong ton kho san pham khong du de xac nhan don hang.");
            }

            product.SoLuongTon -= requiredQuantity;
            product.NgayCapNhat = now;
        }

        foreach (var hold in activeHolds)
        {
            hold.TrangThai = ConfirmedOrderStatus;
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, "Da xac nhan thanh toan va tru ton kho");
        }

        order.TrangThaiDonHang = ConfirmedOrderStatus;
        order.NgayCapNhat = now;
    }

    private static PaymentOrderSummaryDto MapOrderPaymentSummary(Order order)
    {
        var summary = CalculateSummary(order);

        return new PaymentOrderSummaryDto
        {
            MaDonHang = order.MaDonHang,
            MaDonHangKinhDoanh = order.MaDonHangKinhDoanh,
            MaNguoiDung = order.MaNguoiDung,
            LoaiDonHang = order.LoaiDonHang,
            TrangThaiDonHang = order.TrangThaiDonHang,
            TrangThaiThanhToan = order.TrangThaiThanhToan,
            TongThanhToan = order.TongThanhToan,
            TienDatCoc = order.TienDatCoc,
            TongDaThanhToan = summary.TongDaThanhToan,
            SoTienConPhaiThu = summary.SoTienConPhaiThu,
            SoLanThanhToanThanhCong = summary.SoLanThanhToanThanhCong,
            SoLanDangCho = summary.SoLanDangCho,
            NgayThanhToanThanhCong = order.NgayThanhToanThanhCong,
            Payments = order.Payments
                .OrderByDescending(p => p.NgayTao)
                .Select(MapPayment)
                .ToList()
        };
    }

    private static PaymentSummary CalculateSummary(Order order)
    {
        var successfulPayments = order.Payments
            .Where(p => SuccessfulPaymentStatuses.Contains(p.TrangThai))
            .ToList();
        var totalPaid = successfulPayments.Sum(p => p.SoTien);
        var remaining = Math.Max(0, order.TongThanhToan - totalPaid);

        return new PaymentSummary
        {
            TongDaThanhToan = totalPaid,
            SoTienConPhaiThu = remaining,
            SoLanThanhToanThanhCong = successfulPayments.Count,
            SoLanDangCho = order.Payments.Count(p => p.TrangThai == PendingPaymentStatus)
        };
    }

    private static PaymentDto MapPayment(Payment payment)
    {
        return new PaymentDto
        {
            MaThanhToan = payment.MaThanhToan,
            MaThanhToanKinhDoanh = payment.MaThanhToanKinhDoanh,
            MaDonHang = payment.MaDonHang,
            MaDonHangKinhDoanh = payment.Order?.MaDonHangKinhDoanh,
            MaNguoiDung = payment.Order?.MaNguoiDung,
            SoTien = payment.SoTien,
            PhuongThuc = payment.PhuongThuc,
            TrangThai = payment.TrangThai,
            MaGiaoDich = payment.MaGiaoDich,
            DaThanhToanLuc = payment.DaThanhToanLuc,
            NgayTao = payment.NgayTao,
            LoaiThanhToan = payment.LoaiThanhToan,
            NoiDungChuyenKhoan = payment.NoiDungChuyenKhoan,
            MaNganHang = payment.MaNganHang,
            LyDoHuy = payment.LyDoHuy,
            NgayHuy = payment.NgayHuy
        };
    }

    private static string NormalizeAllowedValue(string value, HashSet<string> allowedValues, string errorMessage)
    {
        var match = allowedValues.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            throw new BusinessException(errorMessage);
        }

        return match;
    }

    private static string GeneratePaymentCode()
    {
        return $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..24].ToUpperInvariant();
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string AppendNote(string? current, string note)
    {
        return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
    }

    private sealed class PaymentSummary
    {
        public decimal TongDaThanhToan { get; init; }
        public decimal SoTienConPhaiThu { get; init; }
        public int SoLanThanhToanThanhCong { get; init; }
        public int SoLanDangCho { get; init; }
    }
}
