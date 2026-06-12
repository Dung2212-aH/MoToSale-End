using System.Data;
using OrderService.Data;
using OrderService.DTOs.Cart;
using OrderService.DTOs.Common;
using OrderService.DTOs.Orders;
using OrderService.Entities;
using OrderService.Exceptions;
using OrderService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private const string ActiveUserStatus = "Active";
    private const string ActiveCartStatus = "Active";
    private const string CheckedOutCartStatus = "CheckedOut";
    private const string AvailableProductStatus = "Available";
    private const string AwaitingPaymentStatus = "AwaitingPayment";
    private const string ConfirmedOrderStatus = "Confirmed";
    private const string CancelledOrderStatus = "Cancelled";
    private const string UnpaidStatus = "Unpaid";
    private const string PaidPaymentStatus = "Paid";
    private const string CancelledPaymentStatus = "Cancelled";
    private const string PreparingShippingStatus = "Preparing";
    private const string PartiallyPaidStatus = "PartiallyPaid";
    private const string PendingPaymentRecordStatus = "Pending";
    private const decimal DefaultDeliveryShippingFee = 300000m;

    // Config keys (dbo.HETHONG_CAUHINH)
    private const string CfgBankBin = "BankBin";
    private const string CfgBankAccountNo = "BankAccountNo";
    private const string CfgBankAccountName = "BankAccountName";
    private const string CfgInstallmentAnnualRate = "InstallmentAnnualRate";
    private const string CfgInstallmentMinDownPercent = "InstallmentMinDownPaymentPercent";
    private const string CfgInstallmentAllowedTerms = "InstallmentAllowedTerms";
    private const string CfgPaymentHoldMinutes = "PaymentHoldMinutes";
    private const string CfgDepositMinPercent = "DepositMinPercent";

    private const decimal DefaultInstallmentAnnualRate = 12m;
    private const decimal DefaultInstallmentMinDownPercent = 30m;
    private const decimal DefaultDepositMinPercent = 20m;
    private const int DefaultPaymentHoldMinutes = 1440;
    private static readonly int[] DefaultInstallmentTerms = { 6, 9, 12 };

    private static readonly HashSet<string> AllowedReceiveMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Delivery",
        "Pickup"
    };

    private static readonly HashSet<string> AllowedOrderTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "FullPayment",
        "Deposit",
        "Installment"
    };

    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "COD",
        "BankTransfer",
        "Card",
        "Momo",
        "VNPay"
    };

    private static readonly HashSet<string> CustomerCancelableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        AwaitingPaymentStatus
    };

    private static readonly HashSet<string> AdminCancelBlockedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Delivered",
        "Completed",
        CancelledOrderStatus
    };

    private static readonly HashSet<string> SuccessfulPaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Paid",
        "PartiallyPaid",
        "Refunded"
    };

    private readonly IOrderRepository _orderRepository;
    private readonly ISystemConfigService _config;
    private readonly OrderDbContext _dbContext;

    public OrderService(IOrderRepository orderRepository, ISystemConfigService config, OrderDbContext dbContext)
    {
        _orderRepository = orderRepository;
        _dbContext = dbContext;
        _config = config;
    }

    public async Task<CartDto> GetMyCartAsync(int maNguoiDung)
    {
        await EnsureActiveUserAsync(maNguoiDung);

        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung);
        if (cart is null)
        {
            return EmptyCart(maNguoiDung);
        }

        var imageMap = await _orderRepository.GetPrimaryImageUrlsAsync(cart.Items.Select(i => i.MaSanPham));
        return MapCart(cart, imageMap);
    }

    public async Task<CartDto> AddCartItemAsync(int maNguoiDung, AddCartItemRequest request)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var product = await ValidateProductAsync(request.MaSanPham);
        var variant = await ValidateVariantAsync(product.MaSanPham, request.MaBienSanPham);
        var unitPrice = GetUnitPrice(variant);

        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung);
        var now = DateTime.UtcNow;

        if (cart is null)
        {
            cart = new Cart
            {
                MaNguoiDung = maNguoiDung,
                TrangThai = ActiveCartStatus,
                NgayTao = now,
                NgayCapNhat = now
            };

            await _orderRepository.AddCartAsync(cart);
            await _orderRepository.SaveChangesAsync();
        }

        var existingItem = cart.Items.FirstOrDefault(i =>
            i.MaSanPham == product.MaSanPham &&
            i.MaBienSanPham == variant?.MaBienSanPham);

        var newQuantity = request.SoLuong + (existingItem?.SoLuong ?? 0);
        await EnsureStockAsync(product.MaSanPham, variant?.MaBienSanPham, newQuantity);

        if (existingItem is null)
        {
            await _orderRepository.AddCartItemAsync(new CartItem
            {
                MaGioHang = cart.MaGioHang,
                MaSanPham = product.MaSanPham,
                MaBienSanPham = variant?.MaBienSanPham,
                SoLuong = request.SoLuong,
                DonGia = unitPrice,
                NgayTao = now,
                NgayCapNhat = now
            });
        }
        else
        {
            existingItem.SoLuong = newQuantity;
            existingItem.DonGia = unitPrice;
            existingItem.NgayCapNhat = now;
        }

        cart.NgayCapNhat = now;
        await _orderRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetMyCartAsync(maNguoiDung);
    }

    public async Task<CartDto> UpdateCartItemAsync(int maNguoiDung, int maChiTietGioHang, UpdateCartItemRequest request)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var cartItem = await _orderRepository.GetCartItemForUserAsync(maNguoiDung, maChiTietGioHang)
            ?? throw new NotFoundException("Khong tim thay san pham trong gio hang.");

        var product = await ValidateProductAsync(cartItem.MaSanPham);
        var variant = await ValidateVariantAsync(product.MaSanPham, cartItem.MaBienSanPham);
        await EnsureStockAsync(product.MaSanPham, variant?.MaBienSanPham, request.SoLuong);

        cartItem.SoLuong = request.SoLuong;
        cartItem.DonGia = GetUnitPrice(variant);
        cartItem.NgayCapNhat = DateTime.UtcNow;
        if (cartItem.Cart is not null)
        {
            cartItem.Cart.NgayCapNhat = DateTime.UtcNow;
        }

        await _orderRepository.SaveChangesAsync();
        return await GetMyCartAsync(maNguoiDung);
    }

    public async Task<CartDto> RemoveCartItemAsync(int maNguoiDung, int maChiTietGioHang)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var cartItem = await _orderRepository.GetCartItemForUserAsync(maNguoiDung, maChiTietGioHang)
            ?? throw new NotFoundException("Khong tim thay san pham trong gio hang.");

        var cart = cartItem.Cart;
        _orderRepository.RemoveCartItem(cartItem);
        if (cart is not null)
        {
            cart.NgayCapNhat = DateTime.UtcNow;
        }

        await _orderRepository.SaveChangesAsync();
        return await GetMyCartAsync(maNguoiDung);
    }

    public async Task<CartDto> ClearCartAsync(int maNguoiDung)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung);
        if (cart is null)
        {
            return EmptyCart(maNguoiDung);
        }

        _orderRepository.RemoveCartItems(cart.Items);
        cart.NgayCapNhat = DateTime.UtcNow;
        await _orderRepository.SaveChangesAsync();

        return EmptyCart(maNguoiDung);
    }

    public async Task<ShippingQuoteResponse> GetShippingQuoteAsync(int maNguoiDung, ShippingQuoteRequest request)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung)
            ?? throw new BusinessException("Gio hang dang trong.");

        if (!cart.Items.Any())
        {
            throw new BusinessException("Gio hang dang trong.");
        }

        var method = NormalizeAllowedValue(request.PhuongThucNhanHang, AllowedReceiveMethods);
        return await BuildShippingQuoteAsync(
            maNguoiDung,
            cart.MaGioHang,
            method,
            request.ShippingProvince,
            request.MaVoucherCode,
            strictVoucher: false);
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(int maNguoiDung, CreateOrderFromCartRequest request)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        ValidateOrderRequest(request);

        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        await _orderRepository.CleanupExpiredInventoryHoldsAsync();
        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung)
            ?? throw new BusinessException("Gio hang dang trong.");

        if (!cart.Items.Any())
        {
            throw new BusinessException("Gio hang dang trong.");
        }

        var now = DateTime.UtcNow;
        await RefreshAndValidateCartItemsAsync(cart);

        var subtotal = cart.Items.Sum(i => i.DonGia * i.SoLuong);
        var receiveMethod = NormalizeAllowedValue(request.PhuongThucNhanHang, AllowedReceiveMethods);
        var orderType = NormalizeAllowedValue(request.LoaiDonHang, AllowedOrderTypes);
        var paymentMethod = NormalizePaymentMethod(request.PhuongThucThanhToan);
        var selectedAddress = await GetSelectedDeliveryAddressAsync(maNguoiDung, receiveMethod, request);
        var shippingQuote = await BuildShippingQuoteAsync(
            maNguoiDung,
            cart.MaGioHang,
            receiveMethod,
            selectedAddress?.TinhThanh ?? request.ShippingProvince,
            request.MaVoucherCode,
            strictVoucher: true);
        var voucherDiscount = await GetVoucherDiscountResultAsync(
            maNguoiDung,
            cart.MaGioHang,
            request.MaVoucherCode,
            shippingQuote.OriginalShippingFee,
            strict: true);
        var itemDiscount = voucherDiscount.IsFreeShipping ? 0 : voucherDiscount.Amount;
        var discount = Math.Min(itemDiscount, subtotal);
        var total = subtotal + shippingQuote.ShippingFee - discount;

        var deposit = await GetDepositAmountAsync(orderType, request.TienDatCoc, total);
        var installmentTerm = await ValidateInstallmentTermAsync(orderType, request.SoKyTraGop);

        // Build only the installment application. Monthly collection is handled outside the web app.
        InstallmentPlan? installmentPlan = null;
        if (orderType == "Installment")
        {
            ValidateInstallmentApplication(request.HoSoTraGop);
            await EnsureInstallmentSchemaAsync();
            var annualRate = await _config.GetDecimalAsync(CfgInstallmentAnnualRate, DefaultInstallmentAnnualRate);
            installmentPlan = BuildInstallmentPlan(total, deposit, installmentTerm, annualRate, now, request.HoSoTraGop!);
        }

        var orderRemaining = CalculateInitialOrderRemaining(orderType, total, deposit);

        var holdMinutes = Math.Max(15, await _config.GetIntAsync(CfgPaymentHoldMinutes, DefaultPaymentHoldMinutes));

        var order = new Order
        {
            MaDonHangKinhDoanh = GenerateOrderCode(),
            MaNguoiDung = maNguoiDung,
            HoTenNhanHang = selectedAddress?.HoTenNhanHang.Trim() ?? request.HoTenNhanHang.Trim(),
            SoDienThoaiNhanHang = selectedAddress?.SoDienThoaiNhanHang.Trim() ?? request.SoDienThoaiNhanHang.Trim(),
            EmailNhanHang = string.IsNullOrWhiteSpace(request.EmailNhanHang) ? null : request.EmailNhanHang.Trim().ToLowerInvariant(),
            DiaChiNhanHang = selectedAddress is null ? request.DiaChiNhanHang.Trim() : FormatShippingAddress(selectedAddress),
            TongTienHang = subtotal,
            TienGiam = discount,
            PhiVanChuyen = shippingQuote.ShippingFee,
            TongThanhToan = total,
            TrangThaiDonHang = AwaitingPaymentStatus,
            TrangThaiThanhToan = total == 0 ? PaidPaymentStatus : UnpaidStatus,
            GhiChu = TrimToNull(request.GhiChu),
            NgayTao = now,
            NgayCapNhat = now,
            MaGioHang = cart.MaGioHang,
            PhuongThucNhanHang = receiveMethod,
            TrangThaiVanChuyen = PreparingShippingStatus,
            LoaiDonHang = orderType,
            TienDatCoc = deposit,
            SoTienConLai = orderRemaining,
            NgayHenNhanXe = request.NgayHenNhanXe,
            GhiChuGiaoNhan = TrimToNull(request.GhiChuGiaoNhan),
            Items = cart.Items.Select(MapCartItemToOrderItem).ToList()
        };

        await _orderRepository.AddOrderAsync(order);
        await _orderRepository.SaveChangesAsync();

        foreach (var item in order.Items)
        {
            order.InventoryHolds.Add(new InventoryHold
            {
                MaDonHang = order.MaDonHang,
                MaChiTietDonHang = item.MaChiTietDonHang,
                MaSanPham = item.MaSanPham,
                MaBienSanPham = item.MaBienSanPham,
                SoLuong = item.SoLuong,
                TrangThai = "Active",
                HetHanLuc = now.AddMinutes(holdMinutes),
                NgayTao = now,
                NgayCapNhat = now,
                GhiChu = "Giu ton kho cho thanh toan"
            });
        }

        if (installmentPlan is not null)
        {
            installmentPlan.MaDonHang = order.MaDonHang;
            order.InstallmentPlan = installmentPlan;
        }

        if (total == 0)
        {
            // Free order: nothing to collect, confirm immediately and deduct stock.
            await DeductStockAndConfirmAsync(order, now);
            order.SoTienConLai = 0;
            order.NgayThanhToanThanhCong = now;
        }
        else
        {
            // Create the initial pending payment (the amount the customer must transfer now).
            var initialAmount = orderType == "FullPayment" ? total : deposit;
            var initialType = orderType == "FullPayment" ? "Full" : "Deposit";
            order.Payments.Add(BuildPayment(order, initialAmount, paymentMethod, initialType, now));
        }

        cart.TrangThai = CheckedOutCartStatus;
        cart.NgayCapNhat = now;

        await _orderRepository.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.MaVoucherCode) && voucherDiscount.Amount > 0)
        {
            await _orderRepository.RecordVoucherUseAsync(maNguoiDung, order.MaDonHang, request.MaVoucherCode.Trim(), voucherDiscount.Amount);
        }

        await transaction.CommitAsync();

        var createdOrder = await _orderRepository.GetOrderByIdAsync(order.MaDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang vua tao.");

        return MapOrder(createdOrder);
    }

    /// <summary>
    /// Deducts inventory for the order's active holds, marks the holds Confirmed and moves the
    /// order to Confirmed. Does NOT touch the payment status — callers manage that separately.
    /// Idempotent: if there are no active holds left (already deducted) it does nothing.
    /// </summary>
    private async Task DeductStockAndConfirmAsync(Order order, DateTime now)
    {
        var activeHolds = order.InventoryHolds
            .Where(h => h.TrangThai == "Active")
            .ToList();

        if (activeHolds.Count == 0)
        {
            if (IsInitialOrderStatus(order.TrangThaiDonHang))
            {
                order.TrangThaiDonHang = ConfirmedOrderStatus;
                order.NgayCapNhat = now;
            }
            return;
        }

        foreach (var group in activeHolds.Where(h => h.MaBienSanPham.HasValue).GroupBy(h => h.MaBienSanPham!.Value))
        {
            var variant = await _orderRepository.GetVariantAsync(group.Key)
                ?? throw new BusinessException("Bien the san pham trong don hang khong ton tai.");
            var requiredQuantity = group.Sum(h => h.SoLuong);
            await _orderRepository.ApplyStockMovementAsync(variant.MaSanPham, variant.MaBienSanPham, -requiredQuantity, "BanHang", "Xac nhan don hang va tru ton kho", "Order", order.MaDonHang);
        }

        foreach (var group in activeHolds.Where(h => !h.MaBienSanPham.HasValue).GroupBy(h => h.MaSanPham))
        {
            var product = await _orderRepository.GetProductAsync(group.Key)
                ?? throw new BusinessException("San pham trong don hang khong ton tai.");
            var requiredQuantity = group.Sum(h => h.SoLuong);
            await _orderRepository.ApplyStockMovementAsync(product.MaSanPham, null, -requiredQuantity, "BanHang", "Xac nhan don hang va tru ton kho", "Order", order.MaDonHang);
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

    private static bool IsInitialOrderStatus(string status)
    {
        return status.Equals(AwaitingPaymentStatus, StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Checkout", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PagedResultDto<OrderSummaryDto>> GetOrdersAsync(OrderSearchDto search, int currentUserId, bool canViewAll)
    {
        int? userFilter = canViewAll ? null : currentUserId;
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);

        // Customers don't see "AwaitingPayment" orders in their order list — those are still
        // pending checkout payment and are reached via /checkout/payment instead.
        var hideAwaiting = !canViewAll;
        var orders = await _orderRepository.GetOrdersAsync(search, userFilter, hideAwaiting);
        var totalItems = await _orderRepository.CountOrdersAsync(search, userFilter, hideAwaiting);

        return new PagedResultDto<OrderSummaryDto>
        {
            Items = orders.Select(MapOrderSummary).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<OrderDto> GetOrderByIdAsync(int maDonHang, int currentUserId, bool canViewAll)
    {
        // Defensive: if the user views an order whose installment plan was created before a schema
        // upgrade (e.g. new columns added in a later release), make sure the columns exist before
        // EF tries to SELECT them — otherwise the request fails with "Invalid column name".
        await EnsureInstallmentSchemaAsync();
        var order = await GetOrderForUserAsync(maDonHang, currentUserId, canViewAll);
        return MapOrder(order);
    }

    public async Task<OrderDto> CancelOrderAsync(int maDonHang, int currentUserId, bool canManageAll, CancelOrderRequest request)
    {
        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await GetOrderForUserAsync(maDonHang, currentUserId, canManageAll);
        if (!canManageAll && !CustomerCancelableStatuses.Contains(order.TrangThaiDonHang))
        {
            throw new BusinessException("Don hang hien tai khong the huy.");
        }
        if (canManageAll && AdminCancelBlockedStatuses.Contains(order.TrangThaiDonHang))
        {
            throw new BusinessException("Don hang hien tai khong the huy.");
        }
        if (canManageAll && string.IsNullOrWhiteSpace(request.LyDoHuyDon))
        {
            throw new BusinessException("Ly do huy don la bat buoc.");
        }

        var now = DateTime.UtcNow;
        order.TrangThaiDonHang = CancelledOrderStatus;
        if (!SuccessfulPaymentStatuses.Contains(order.TrangThaiThanhToan))
        {
            order.TrangThaiThanhToan = CancelledPaymentStatus;
        }
        order.NgayHuyDon = now;
        order.LyDoHuyDon = TrimToNull(request.LyDoHuyDon) ?? "Khach hang huy don";
        order.NgayCapNhat = now;

        await ReleaseInventoryForCancelledOrderAsync(order, now, "Huy don, nha giu cho", "Huy don, hoan ton kho");

        foreach (var pending in order.Payments.Where(p => p.TrangThai == PendingPaymentRecordStatus))
        {
            pending.TrangThai = CancelledPaymentStatus;
            pending.NgayHuy = now;
            pending.LyDoHuy = "Huy don hang";
        }

        if (order.InstallmentPlan is not null && order.InstallmentPlan.TrangThai != "Cancelled")
        {
            order.InstallmentPlan.TrangThai = "Cancelled";
            order.InstallmentPlan.NgayCapNhat = now;
        }

        await _orderRepository.SaveChangesAsync();

        if (order.Vouchers.Any())
        {
            await _orderRepository.CancelVoucherUseAsync(order.MaDonHang);
        }

        await transaction.CommitAsync();

        var updatedOrder = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        return MapOrder(updatedOrder);
    }

    public async Task<PaymentInfoDto> GetPaymentInfoAsync(int maDonHang, int currentUserId, bool canViewAll)
    {
        var order = await GetOrderForUserAsync(maDonHang, currentUserId, canViewAll);
        var amountDue = ComputeAmountDueNow(order);
        var content = order.MaDonHangKinhDoanh;

        var bin = await _config.GetStringAsync(CfgBankBin);
        var accountNo = await _config.GetStringAsync(CfgBankAccountNo);
        var accountName = await _config.GetStringAsync(CfgBankAccountName);
        var configured = !string.IsNullOrWhiteSpace(bin) && !string.IsNullOrWhiteSpace(accountNo);

        return new PaymentInfoDto
        {
            MaDonHang = order.MaDonHang,
            MaDonHangKinhDoanh = order.MaDonHangKinhDoanh,
            TrangThaiDonHang = order.TrangThaiDonHang,
            TrangThaiThanhToan = order.TrangThaiThanhToan,
            LoaiDonHang = order.LoaiDonHang,
            TongThanhToan = order.TongThanhToan,
            SoTienCanThanhToan = amountDue,
            NoiDungChuyenKhoan = content,
            DaCauHinhNganHang = configured,
            TenNganHang = bin,
            SoTaiKhoan = accountNo,
            ChuTaiKhoan = accountName,
            QrImageUrl = configured && amountDue > 0
                ? BuildVietQrUrl(bin!, accountNo!, accountName, amountDue, content)
                : null
        };
    }

    public async Task<OrderDto> ConfirmOrderPaymentAsync(int maDonHang, ConfirmOrderPaymentRequest request)
    {
        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        if (string.Equals(order.TrangThaiDonHang, CancelledOrderStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Don hang da huy, khong the xac nhan thanh toan.");
        }

        var now = DateTime.UtcNow;
        var txnRef = TrimToNull(request.MaGiaoDich);
        var method = PaymentMethodOf(order);

        var pending = order.Payments
            .Where(p => p.TrangThai == PendingPaymentRecordStatus)
            .OrderBy(p => p.NgayTao)
            .ThenBy(p => p.MaThanhToan)
            .FirstOrDefault();

        if (pending is not null)
        {
            pending.TrangThai = PaidPaymentStatus;
            pending.DaThanhToanLuc = now;
            if (txnRef is not null)
            {
                pending.MaGiaoDich = txnRef;
            }
        }
        else if (string.Equals(order.LoaiDonHang, "Installment", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Ho so tra gop da duoc duyet, khong thu tien hang thang tren web.");
        }
        else
        {
            var remaining = ComputeOrderRemaining(order);
            if (remaining <= 0)
            {
                throw new BusinessException("Don hang da thanh toan du.");
            }

            order.Payments.Add(ConfirmedPayment(order, remaining, method, "Remaining", now, txnRef));
        }

        // The first successful payment confirms the order and deducts the reserved stock.
        if (IsInitialOrderStatus(order.TrangThaiDonHang))
        {
            await DeductStockAndConfirmAsync(order, now);
        }

        RecomputeOrderPaymentState(order, now);

        if (!string.IsNullOrWhiteSpace(request.GhiChu))
        {
            order.GhiChu = AppendNote(order.GhiChu, request.GhiChu.Trim());
        }

        order.NgayCapNhat = now;

        await _orderRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        var updated = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        return MapOrder(updated);
    }

    /// <summary>
    /// Customer-initiated refund request. Only valid for orders that have already received money
    /// (PartiallyPaid / Paid) and haven't shipped yet. Cancels the order, releases inventory, and
    /// records the customer's bank account so admin knows where to send the refund.
    /// </summary>
    public async Task<OrderDto> RequestRefundAsync(int maDonHang, int currentUserId, CreateRefundRequestDto request)
    {
        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        if (order.MaNguoiDung != currentUserId)
        {
            throw new ForbiddenException("Ban khong co quyen yeu cau hoan tien cho don hang nay.");
        }

        if (string.Equals(order.TrangThaiDonHang, CancelledOrderStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Don hang da huy.");
        }

        if (!SuccessfulPaymentStatuses.Contains(order.TrangThaiThanhToan))
        {
            throw new BusinessException("Don hang chua co giao dich thanh toan thanh cong de hoan tien.");
        }

        // Don't allow refund once the order has shipped or been delivered — that's a return, not a refund.
        if (order.TrangThaiDonHang.Equals("Shipping", StringComparison.OrdinalIgnoreCase) ||
            order.TrangThaiDonHang.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ||
            order.TrangThaiDonHang.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Don hang da giao, vui long lien he cua hang de doi/tra.");
        }

        if (order.RefundRequests.Any(r => r.TrangThai == "Pending"))
        {
            throw new BusinessException("Don hang da co yeu cau hoan tien dang cho xu ly.");
        }

        var now = DateTime.UtcNow;
        var refundAmount = order.Payments
            .Where(p => p.TrangThai == PaidPaymentStatus)
            .Sum(p => p.SoTien);

        order.RefundRequests.Add(new RefundRequest
        {
            MaDonHang = order.MaDonHang,
            SoTien = refundAmount,
            TenNganHang = request.TenNganHang.Trim(),
            SoTaiKhoan = request.SoTaiKhoan.Trim(),
            ChuTaiKhoan = request.ChuTaiKhoan.Trim().ToUpperInvariant(),
            LyDo = TrimToNull(request.LyDo),
            TrangThai = "Pending",
            NgayTao = now
        });

        // Cancel the order itself: release inventory holds, mark cancelled, keep payment status as
        // Paid/PartiallyPaid until admin actually transfers money back (then it becomes Refunded).
        order.TrangThaiDonHang = CancelledOrderStatus;
        order.NgayHuyDon = now;
        order.LyDoHuyDon = TrimToNull(request.LyDo) ?? "Khach yeu cau huy va hoan tien";
        order.NgayCapNhat = now;

        await ReleaseInventoryForCancelledOrderAsync(order, now, "Huy don, nha giu cho", "Huy don, hoan ton kho");

        foreach (var pending in order.Payments.Where(p => p.TrangThai == PendingPaymentRecordStatus))
        {
            pending.TrangThai = CancelledPaymentStatus;
            pending.NgayHuy = now;
            pending.LyDoHuy = "Huy don va yeu cau hoan tien";
        }

        if (order.InstallmentPlan is not null && order.InstallmentPlan.TrangThai != "Cancelled")
        {
            order.InstallmentPlan.TrangThai = "Cancelled";
            order.InstallmentPlan.NgayCapNhat = now;
        }

        await _orderRepository.SaveChangesAsync();

        if (order.Vouchers.Any())
        {
            await _orderRepository.CancelVoucherUseAsync(order.MaDonHang);
        }

        await transaction.CommitAsync();

        var updated = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");
        return MapOrder(updated);
    }

    /// <summary>
    /// Admin marks the refund as completed (after they've actually wired the money to the
    /// customer's account). Moves the order's payment status to Refunded.
    /// </summary>
    public async Task<OrderDto> ConfirmRefundAsync(int maDonHang, int maYeuCauHoanTien, ConfirmRefundRequest request)
    {
        await using var transaction = await _orderRepository.BeginTransactionAsync(IsolationLevel.Serializable);

        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        var refund = order.RefundRequests.FirstOrDefault(r => r.MaYeuCauHoanTien == maYeuCauHoanTien)
            ?? throw new NotFoundException("Khong tim thay yeu cau hoan tien.");

        if (refund.TrangThai != "Pending")
        {
            throw new BusinessException("Yeu cau hoan tien nay khong o trang thai cho xu ly.");
        }

        var now = DateTime.UtcNow;
        refund.TrangThai = "Completed";
        refund.NgayHoanTat = now;
        refund.MaGiaoDichHoan = TrimToNull(request.MaGiaoDichHoan);
        refund.GhiChuAdmin = TrimToNull(request.GhiChuAdmin);

        order.TrangThaiThanhToan = "Refunded";
        order.NgayCapNhat = now;

        await _orderRepository.SaveChangesAsync();
        await transaction.CommitAsync();

        var updated = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");
        return MapOrder(updated);
    }

    private void RecomputeOrderPaymentState(Order order, DateTime now)
    {
        // CK_DONHANG_DatCoc locks the relationship between LoaiDonHang / TienDatCoc / SoTienConLai
        // (FullPayment ⇒ both = 0; Deposit ⇒ deposit ∈ (0,total); Installment ⇒ SoTienConLai > 0).
        // So we DO NOT mutate SoTienConLai here — it was set correctly at order creation and is a
        // structural value, not the real outstanding amount. The real outstanding amount is derived
        // from Payments + InstallmentPlan via ComputeAmountDueNow / ComputeOrderRemaining at read time.
        var total = order.TongThanhToan;

        if (string.Equals(order.LoaiDonHang, "Installment", StringComparison.OrdinalIgnoreCase) && order.InstallmentPlan is not null)
        {
            var plan = order.InstallmentPlan;
            var downPaid = DownPaymentPaid(order);

            if (downPaid)
            {
                order.TrangThaiThanhToan = PartiallyPaidStatus;
                plan.TrangThai = "Approved";
                plan.NgayCapNhat = now;
            }
            else
            {
                order.TrangThaiThanhToan = UnpaidStatus;
            }

            return;
        }

        var paid = order.Payments.Where(p => p.TrangThai == PaidPaymentStatus).Sum(p => p.SoTien);
        if (total <= 0 || paid >= total)
        {
            order.TrangThaiThanhToan = PaidPaymentStatus;
            order.NgayThanhToanThanhCong ??= now;
        }
        else if (paid > 0)
        {
            order.TrangThaiThanhToan = PartiallyPaidStatus;
        }
        else
        {
            order.TrangThaiThanhToan = UnpaidStatus;
        }
    }

    private static decimal ComputeAmountDueNow(Order order)
    {
        if (string.Equals(order.TrangThaiDonHang, CancelledOrderStatus, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var pending = order.Payments
            .Where(p => p.TrangThai == PendingPaymentRecordStatus)
            .OrderBy(p => p.NgayTao)
            .FirstOrDefault();
        if (pending is not null)
        {
            return pending.SoTien;
        }

        if (string.Equals(order.LoaiDonHang, "Installment", StringComparison.OrdinalIgnoreCase) && order.InstallmentPlan is not null)
        {
            return 0;
        }

        return ComputeOrderRemaining(order);
    }

    private static decimal CalculateInitialOrderRemaining(string orderType, decimal total, decimal deposit)
    {
        return orderType switch
        {
            "FullPayment" => 0,
            "Deposit" or "Installment" => Math.Max(0, total - deposit),
            _ => Math.Max(0, total)
        };
    }

    private static decimal ComputeOrderRemaining(Order order)
    {
        var paid = order.Payments.Where(p => p.TrangThai == PaidPaymentStatus).Sum(p => p.SoTien);
        return Math.Max(0, order.TongThanhToan - paid);
    }

    private static bool DownPaymentPaid(Order order)
    {
        return order.Payments.Any(p =>
            p.TrangThai == PaidPaymentStatus &&
            (p.LoaiThanhToan == "Deposit" || p.LoaiThanhToan == "Full"));
    }

    private static string PaymentMethodOf(Order order)
    {
        return order.Payments
            .OrderByDescending(p => p.NgayTao)
            .ThenByDescending(p => p.MaThanhToan)
            .Select(p => p.PhuongThuc)
            .FirstOrDefault() ?? "BankTransfer";
    }

    private static Payment ConfirmedPayment(Order order, decimal amount, string method, string type, DateTime now, string? txnRef)
    {
        return new Payment
        {
            MaDonHang = order.MaDonHang,
            MaThanhToanKinhDoanh = GeneratePaymentCode(),
            SoTien = amount,
            PhuongThuc = method,
            TrangThai = PaidPaymentStatus,
            LoaiThanhToan = type,
            NoiDungChuyenKhoan = order.MaDonHangKinhDoanh,
            MaGiaoDich = txnRef,
            DaThanhToanLuc = now,
            NgayTao = now
        };
    }

    private static string BuildVietQrUrl(string bin, string accountNo, string? accountName, decimal amount, string content)
    {
        var amt = (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        var info = Uri.EscapeDataString(content);
        var name = Uri.EscapeDataString(accountName ?? string.Empty);
        return $"https://img.vietqr.io/image/{bin}-{accountNo}-compact2.png?amount={amt}&addInfo={info}&accountName={name}";
    }

    private async Task EnsureActiveUserAsync(int maNguoiDung)
    {
        var user = await _orderRepository.GetUserAsync(maNguoiDung)
            ?? throw new NotFoundException("Khong tim thay nguoi dung.");

        if (!string.Equals(user.TrangThai, ActiveUserStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Tai khoan khong o trang thai Active.");
        }
    }

    private async Task<Product> ValidateProductAsync(int maSanPham)
    {
        var product = await _orderRepository.GetProductAsync(maSanPham)
            ?? throw new NotFoundException("San pham khong ton tai.");

        if (!product.DangHoatDong || !string.Equals(product.TrangThaiSanPham, AvailableProductStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("San pham khong kha dung.");
        }

        return product;
    }

    private async Task<ProductVariant?> ValidateVariantAsync(int maSanPham, int? maBienSanPham)
    {
        if (!maBienSanPham.HasValue)
        {
            // Giá nằm ở biến thể: tự chọn biến thể mặc định nếu sản phẩm chỉ có 1 biến thể đang bán.
            var sellable = (await _orderRepository.GetVariantsByProductAsync(maSanPham))
                .Where(v => string.Equals(v.TrangThai, AvailableProductStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (sellable.Count == 1)
            {
                return sellable[0];
            }

            if (sellable.Count == 0)
            {
                throw new BusinessException("San pham chua co bien the kha dung de ban.");
            }

            throw new BusinessException("Vui long chon phien ban/mau sac truoc khi them vao gio hang.");
        }

        var variant = await _orderRepository.GetVariantAsync(maBienSanPham.Value)
            ?? throw new NotFoundException("Bien the san pham khong ton tai.");

        if (variant.MaSanPham != maSanPham)
        {
            throw new BusinessException("Bien the khong thuoc san pham.");
        }

        if (!string.Equals(variant.TrangThai, AvailableProductStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Bien the san pham khong kha dung.");
        }

        return variant;
    }

    private async Task EnsureStockAsync(int maSanPham, int? maBienSanPham, int requiredQuantity)
    {
        var availableStock = await _orderRepository.GetAvailableStockAsync(maSanPham, maBienSanPham);
        if (requiredQuantity > availableStock)
        {
            throw new BusinessException("So luong ton kho kha dung khong du.");
        }
    }

    private async Task RefreshAndValidateCartItemsAsync(Cart cart)
    {
        foreach (var item in cart.Items)
        {
            var product = await ValidateProductAsync(item.MaSanPham);
            var variant = await ValidateVariantAsync(product.MaSanPham, item.MaBienSanPham);
            await EnsureStockAsync(product.MaSanPham, variant?.MaBienSanPham, item.SoLuong);

            item.Product = product;
            item.Variant = variant;
            item.DonGia = GetUnitPrice(variant);
            item.NgayCapNhat = DateTime.UtcNow;
        }
    }

    private async Task<ShippingQuoteResponse> BuildShippingQuoteAsync(
        int maNguoiDung,
        int maGioHang,
        string receiveMethod,
        string? province,
        string? voucherCode,
        bool strictVoucher)
    {
        if (receiveMethod.Equals("Pickup", StringComparison.OrdinalIgnoreCase))
        {
            return new ShippingQuoteResponse
            {
                ShippingFee = 0,
                OriginalShippingFee = 0,
                DiscountAmount = 0,
                IsFreeShipping = true,
                FreeReason = "Pickup"
            };
        }

        var baseQuote = await GetBaseShippingQuoteAsync(province);
        var voucherDiscount = await GetVoucherDiscountResultAsync(
            maNguoiDung,
            maGioHang,
            voucherCode,
            baseQuote.OriginalShippingFee,
            strictVoucher);
        var shippingDiscount = voucherDiscount.IsFreeShipping
            ? Math.Min(voucherDiscount.Amount, baseQuote.OriginalShippingFee)
            : 0;

        baseQuote.DiscountAmount = shippingDiscount;
        baseQuote.ShippingFee = Math.Max(0, baseQuote.OriginalShippingFee - shippingDiscount);
        baseQuote.IsFreeShipping = baseQuote.ShippingFee == 0 && baseQuote.OriginalShippingFee > 0;
        baseQuote.FreeReason = shippingDiscount > 0 ? "FreeShippingVoucher" : null;
        return baseQuote;
    }

    private static Task<ShippingQuoteResponse> GetBaseShippingQuoteAsync(string? province)
    {
        return Task.FromResult(new ShippingQuoteResponse
        {
            ShippingFee = DefaultDeliveryShippingFee,
            OriginalShippingFee = DefaultDeliveryShippingFee,
            CarrierCode = "STANDARD",
            CarrierName = "Phí vận chuyển mặc định"
        });
    }

    private async Task<VoucherDiscountResult> GetVoucherDiscountResultAsync(int maNguoiDung, int maGioHang, string? maVoucherCode, decimal phiVanChuyen, bool strict)
    {
        if (string.IsNullOrWhiteSpace(maVoucherCode))
        {
            return VoucherDiscountResult.Empty;
        }

        var code = maVoucherCode.Trim();
        if (!await _orderRepository.UserHasSavedVoucherAsync(maNguoiDung, code))
        {
            if (strict)
            {
                throw new BusinessException("Ban chua nhan voucher nay.");
            }

            return VoucherDiscountResult.Empty;
        }

        var voucher = await _orderRepository.ValidateVoucherAsync(maNguoiDung, maGioHang, code, phiVanChuyen)
            ?? throw new BusinessException("Khong kiem tra duoc voucher.");

        if (!voucher.HopLe)
        {
            if (strict)
            {
                throw new BusinessException(voucher.LyDoKhongHopLe ?? "Voucher khong hop le.");
            }

            return VoucherDiscountResult.Empty;
        }

        return new VoucherDiscountResult(voucher.SoTienGiam, string.Equals(voucher.LoaiGiamGia, "FreeShipping", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<Order> GetOrderForUserAsync(int maDonHang, int currentUserId, bool canViewAll)
    {
        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        if (!canViewAll && order.MaNguoiDung != currentUserId)
        {
            throw new ForbiddenException("Ban khong co quyen truy cap don hang nay.");
        }

        return order;
    }

    private async Task ReleaseInventoryForCancelledOrderAsync(
        Order order,
        DateTime now,
        string activeHoldNote,
        string confirmedHoldNote)
    {
        foreach (var group in order.InventoryHolds
            .Where(h => h.TrangThai == ConfirmedOrderStatus && h.MaBienSanPham.HasValue)
            .GroupBy(h => h.MaBienSanPham!.Value))
        {
            var variant = await _orderRepository.GetVariantAsync(group.Key)
                ?? throw new BusinessException("Bien the san pham trong don hang khong ton tai.");

            await _orderRepository.ApplyStockMovementAsync(variant.MaSanPham, variant.MaBienSanPham, group.Sum(h => h.SoLuong), "HoanTon", confirmedHoldNote, "OrderCancel", order.MaDonHang);
        }

        foreach (var group in order.InventoryHolds
            .Where(h => h.TrangThai == ConfirmedOrderStatus && !h.MaBienSanPham.HasValue)
            .GroupBy(h => h.MaSanPham))
        {
            var product = await _orderRepository.GetProductAsync(group.Key)
                ?? throw new BusinessException("San pham trong don hang khong ton tai.");

            await _orderRepository.ApplyStockMovementAsync(product.MaSanPham, null, group.Sum(h => h.SoLuong), "HoanTon", confirmedHoldNote, "OrderCancel", order.MaDonHang);
        }

        foreach (var hold in order.InventoryHolds.Where(h => h.TrangThai is "Active" or ConfirmedOrderStatus))
        {
            var note = hold.TrangThai == ConfirmedOrderStatus ? confirmedHoldNote : activeHoldNote;
            hold.TrangThai = "Cancelled";
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, note);
        }
    }

    private static void ValidateOrderRequest(CreateOrderFromCartRequest request)
    {
        if (!AllowedReceiveMethods.Contains(request.PhuongThucNhanHang))
        {
            throw new BusinessException("Phuong thuc nhan hang khong hop le.");
        }

        if (!AllowedOrderTypes.Contains(request.LoaiDonHang))
        {
            throw new BusinessException("Loai don hang khong hop le.");
        }
    }

    private async Task<UserAddress?> GetSelectedDeliveryAddressAsync(
        int maNguoiDung,
        string receiveMethod,
        CreateOrderFromCartRequest request)
    {
        if (!receiveMethod.Equals("Delivery", StringComparison.OrdinalIgnoreCase) || !request.MaDiaChiNhanHang.HasValue)
        {
            return null;
        }

        return await _orderRepository.GetUserAddressAsync(maNguoiDung, request.MaDiaChiNhanHang.Value)
            ?? throw new BusinessException("Dia chi nhan hang khong hop le.");
    }

    private static string FormatShippingAddress(UserAddress address)
    {
        return string.Join(", ", new[]
        {
            address.DiaChiNhanHang,
            address.PhuongXa,
            address.QuanHuyen,
            address.TinhThanh
        }.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));
    }

    private async Task<decimal> GetDepositAmountAsync(string orderType, decimal requestedDeposit, decimal total)
    {
        if (orderType == "FullPayment")
        {
            return 0;
        }

        if (orderType == "Deposit")
        {
            if (requestedDeposit <= 0)
            {
                throw new BusinessException("Vui long nhap so tien dat coc.");
            }

            var minPercent = await _config.GetDecimalAsync(CfgDepositMinPercent, DefaultDepositMinPercent);
            var minDeposit = Math.Round(total * minPercent / 100m, 0, MidpointRounding.AwayFromZero);
            if (requestedDeposit < minDeposit)
            {
                throw new BusinessException($"Tien dat coc phai it nhat {minPercent:0.#}% tong don ({minDeposit:#,##0} d).");
            }

            if (requestedDeposit >= total)
            {
                throw new BusinessException("Tien dat coc phai nho hon tong thanh toan.");
            }

            return requestedDeposit;
        }

        if (orderType == "Installment")
        {
            var minPercent = await _config.GetDecimalAsync(CfgInstallmentMinDownPercent, DefaultInstallmentMinDownPercent);
            var minDeposit = Math.Round(total * minPercent / 100m, 0, MidpointRounding.AwayFromZero);
            if (requestedDeposit < minDeposit)
            {
                throw new BusinessException($"Tien tra truoc phai it nhat {minPercent:0.#}% tong don ({minDeposit:#,##0} d).");
            }

            if (requestedDeposit >= total)
            {
                throw new BusinessException("Tien tra truoc phai nho hon tong thanh toan.");
            }

            return requestedDeposit;
        }

        return requestedDeposit;
    }

    private static void ValidateInstallmentApplication(InstallmentApplicationDto? application)
    {
        if (application is null)
        {
            throw new BusinessException("Vui long dien ho so tra gop.");
        }
        if (string.IsNullOrWhiteSpace(application.HoTenNguoiVay))
        {
            throw new BusinessException("Vui long nhap ho ten nguoi vay.");
        }
        if (string.IsNullOrWhiteSpace(application.SoCCCD))
        {
            throw new BusinessException("Vui long nhap so CCCD/CMND.");
        }
        if (!application.NgayCapCCCD.HasValue)
        {
            throw new BusinessException("Vui long nhap ngay cap CCCD.");
        }
        if (string.IsNullOrWhiteSpace(application.NoiCapCCCD))
        {
            throw new BusinessException("Vui long nhap noi cap CCCD.");
        }
        if (string.IsNullOrWhiteSpace(application.SoDienThoai))
        {
            throw new BusinessException("Vui long nhap so dien thoai nguoi vay.");
        }
        if (string.IsNullOrWhiteSpace(application.DiaChiThuongTru))
        {
            throw new BusinessException("Vui long nhap dia chi thuong tru.");
        }
    }

    /// <summary>
    /// Ensure the installment-plan tables exist with the expected columns before any EF read or
    /// write touches them. Idempotent and safe to call on startup or per-request. Keeps the
    /// schema minimal: only the four columns actually consumed by the customer-facing application.
    /// </summary>
    private async Task EnsureInstallmentSchemaAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'dbo.HOSO_TRAGOP', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HOSO_TRAGOP(
                    MaHoSoTraGop INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    MaDonHang INT NOT NULL,
                    TienTraTruoc DECIMAL(18,2) NOT NULL,
                    SoTienGoc DECIMAL(18,2) NOT NULL,
                    SoKy INT NOT NULL,
                    LaiSuatNam DECIMAL(9,4) NOT NULL,
                    TongTienLai DECIMAL(18,2) NOT NULL,
                    TongPhaiTra DECIMAL(18,2) NOT NULL,
                    TrangThai VARCHAR(20) NOT NULL,
                    NgayTao DATETIME2(0) NOT NULL,
                    NgayCapNhat DATETIME2(0) NOT NULL,
                    HoTenNguoiVay NVARCHAR(150) NOT NULL DEFAULT(N''),
                    SoCCCD VARCHAR(20) NOT NULL DEFAULT(''),
                    NgheNghiep NVARCHAR(100) NULL,
                    ThuNhapHangThang DECIMAL(18,2) NULL,
                    CONSTRAINT FK_HOSO_TRAGOP_DONHANG FOREIGN KEY (MaDonHang) REFERENCES dbo.DONHANG(MaDonHang),
                    CONSTRAINT UQ_HOSO_TRAGOP_DONHANG UNIQUE (MaDonHang)
                );
            END
            ELSE
            BEGIN
                IF COL_LENGTH('dbo.HOSO_TRAGOP','HoTenNguoiVay') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD HoTenNguoiVay NVARCHAR(150) NOT NULL CONSTRAINT DF_HOSO_TRAGOP_HoTen DEFAULT(N'');
                IF COL_LENGTH('dbo.HOSO_TRAGOP','SoCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD SoCCCD VARCHAR(20) NOT NULL CONSTRAINT DF_HOSO_TRAGOP_CCCD DEFAULT('');
                IF COL_LENGTH('dbo.HOSO_TRAGOP','NgheNghiep') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgheNghiep NVARCHAR(100) NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','ThuNhapHangThang') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD ThuNhapHangThang DECIMAL(18,2) NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','NgaySinh') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgaySinh DATE NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','SoDienThoai') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD SoDienThoai VARCHAR(20) NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','DiaChiThuongTru') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD DiaChiThuongTru NVARCHAR(255) NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','TenCongTy') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD TenCongTy NVARCHAR(150) NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','ThoiGianLamViecThang') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD ThoiGianLamViecThang INT NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','NgayCapCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NgayCapCCCD DATE NULL;
                IF COL_LENGTH('dbo.HOSO_TRAGOP','NoiCapCCCD') IS NULL ALTER TABLE dbo.HOSO_TRAGOP ADD NoiCapCCCD NVARCHAR(150) NULL;
                -- Legacy columns NguoiThamChieu/SDTThamChieu/QuanHeThamChieu left in place if present
                -- (EF no longer reads them); safe to drop manually later if desired.
            END;

            IF OBJECT_ID(N'dbo.KY_TRAGOP', N'U') IS NOT NULL
                DROP TABLE dbo.KY_TRAGOP;
            """);
    }

    private async Task<int> ValidateInstallmentTermAsync(string orderType, int? requestedTerm)
    {
        if (orderType != "Installment")
        {
            return 0;
        }

        var allowed = await GetAllowedInstallmentTermsAsync();
        if (!requestedTerm.HasValue || !allowed.Contains(requestedTerm.Value))
        {
            throw new BusinessException($"So ky tra gop khong hop le. Chi chap nhan: {string.Join(", ", allowed)} thang.");
        }

        return requestedTerm.Value;
    }

    private async Task<int[]> GetAllowedInstallmentTermsAsync()
    {
        var raw = await _config.GetStringAsync(CfgInstallmentAllowedTerms);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DefaultInstallmentTerms;
        }

        var parsed = raw
            .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var v) ? v : 0)
            .Where(v => v > 0)
            .Distinct()
            .OrderBy(v => v)
            .ToArray();

        return parsed.Length > 0 ? parsed : DefaultInstallmentTerms;
    }

    private static InstallmentPlan BuildInstallmentPlan(decimal total, decimal deposit, int term, decimal annualRate, DateTime now, InstallmentApplicationDto application)
    {
        var principal = Math.Max(0, total - deposit);
        // Flat interest on the financed principal: principal * rate% * (months / 12).
        var interestTotal = Math.Round(principal * annualRate / 100m * term / 12m, 0, MidpointRounding.AwayFromZero);
        var totalPayable = principal + interestTotal;

        var plan = new InstallmentPlan
        {
            TienTraTruoc = deposit,
            SoTienGoc = principal,
            SoKy = term,
            LaiSuatNam = annualRate,
            TongTienLai = interestTotal,
            TongPhaiTra = totalPayable,
            TrangThai = "Pending",
            NgayTao = now,
            NgayCapNhat = now,
            HoTenNguoiVay = application.HoTenNguoiVay.Trim(),
            SoCCCD = application.SoCCCD.Trim(),
            NgayCapCCCD = application.NgayCapCCCD,
            NoiCapCCCD = application.NoiCapCCCD.Trim(),
            NgaySinh = application.NgaySinh,
            SoDienThoai = application.SoDienThoai.Trim(),
            DiaChiThuongTru = application.DiaChiThuongTru.Trim(),
            NgheNghiep = TrimToNull(application.NgheNghiep),
            TenCongTy = TrimToNull(application.TenCongTy),
            ThoiGianLamViecThang = application.ThoiGianLamViecThang,
            ThuNhapHangThang = application.ThuNhapHangThang
        };

        return plan;
    }

    private static Payment BuildPayment(Order order, decimal amount, string method, string type, DateTime now)
    {
        return new Payment
        {
            MaDonHang = order.MaDonHang,
            MaThanhToanKinhDoanh = GeneratePaymentCode(),
            SoTien = amount,
            PhuongThuc = method,
            TrangThai = PendingPaymentRecordStatus,
            LoaiThanhToan = type,
            NoiDungChuyenKhoan = order.MaDonHangKinhDoanh,
            NgayTao = now
        };
    }

    private static string NormalizePaymentMethod(string? value)
    {
        var match = AllowedPaymentMethods.FirstOrDefault(x => x.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? "BankTransfer";
    }

    private static string GeneratePaymentCode()
    {
        return $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..24].ToUpperInvariant();
    }

    private static OrderItem MapCartItemToOrderItem(CartItem item)
    {
        return new OrderItem
        {
            MaSanPham = item.MaSanPham,
            MaBienSanPham = item.MaBienSanPham,
            TenSanPhamSnapshot = item.Product?.TenSanPham ?? string.Empty,
            SKUSnapshot = item.Variant?.SKU,
            DonGia = item.DonGia,
            SoLuong = item.SoLuong
        };
    }

    private static CartDto MapCart(Cart cart, IReadOnlyDictionary<int, string> imageMap)
    {
        var items = cart.Items
            .OrderBy(i => i.NgayTao)
            .Select(item => MapCartItem(item, imageMap))
            .ToList();

        return new CartDto
        {
            MaGioHang = cart.MaGioHang,
            MaNguoiDung = cart.MaNguoiDung,
            TrangThai = cart.TrangThai,
            Items = items,
            TongSoLuong = items.Sum(i => i.SoLuong),
            TongTienHang = items.Sum(i => i.ThanhTien)
        };
    }

    private static CartItemDto MapCartItem(CartItem item, IReadOnlyDictionary<int, string> imageMap)
    {
        // Ưu tiên ảnh suy ra từ ANHSANPHAM (khớp ảnh ở trang sản phẩm), fallback cột denormalized.
        var anhChinhUrl = imageMap.TryGetValue(item.MaSanPham, out var url) ? url : item.Product?.AnhChinhUrl;

        return new CartItemDto
        {
            MaChiTietGioHang = item.MaChiTietGioHang,
            MaSanPham = item.MaSanPham,
            MaBienSanPham = item.MaBienSanPham,
            TenSanPham = item.Product?.TenSanPham ?? string.Empty,
            TenBienThe = item.Variant?.TenBienThe,
            SKU = item.Variant?.SKU,
            SoLuong = item.SoLuong,
            DonGia = item.DonGia,
            ThanhTien = item.DonGia * item.SoLuong,
            AnhChinhUrl = anhChinhUrl
        };
    }

    private static CartDto EmptyCart(int maNguoiDung)
    {
        return new CartDto
        {
            MaNguoiDung = maNguoiDung,
            TrangThai = ActiveCartStatus
        };
    }

    private static OrderSummaryDto MapOrderSummary(Order order)
    {
        return new OrderSummaryDto
        {
            MaDonHang = order.MaDonHang,
            MaDonHangKinhDoanh = order.MaDonHangKinhDoanh,
            MaNguoiDung = order.MaNguoiDung,
            HoTenNhanHang = order.HoTenNhanHang,
            EmailNhanHang = order.EmailNhanHang,
            SoDienThoaiNhanHang = order.SoDienThoaiNhanHang,
            PhiVanChuyen = order.PhiVanChuyen,
            TongThanhToan = order.TongThanhToan,
            TrangThaiDonHang = order.TrangThaiDonHang,
            TrangThaiThanhToan = order.TrangThaiThanhToan,
            TrangThaiVanChuyen = order.TrangThaiVanChuyen,
            LoaiDonHang = order.LoaiDonHang,
            NgayTao = order.NgayTao,
            NgayThanhToanThanhCong = order.NgayThanhToanThanhCong,
            Items = order.Items.Select(MapOrderItem).ToList()
        };
    }

    private static OrderDto MapOrder(Order order)
    {
        var activeHoldExpiry = order.InventoryHolds
            .Where(h => h.TrangThai == "Active")
            .Select(h => (DateTime?)h.HetHanLuc)
            .DefaultIfEmpty()
            .Min();

        return new OrderDto
        {
            MaDonHang = order.MaDonHang,
            MaDonHangKinhDoanh = order.MaDonHangKinhDoanh,
            MaNguoiDung = order.MaNguoiDung,
            MaGioHang = order.MaGioHang,
            HoTenNhanHang = order.HoTenNhanHang,
            SoDienThoaiNhanHang = order.SoDienThoaiNhanHang,
            EmailNhanHang = order.EmailNhanHang,
            DiaChiNhanHang = order.DiaChiNhanHang,
            TongTienHang = order.TongTienHang,
            TienGiam = order.TienGiam,
            PhiVanChuyen = order.PhiVanChuyen,
            TongThanhToan = order.TongThanhToan,
            TrangThaiDonHang = order.TrangThaiDonHang,
            TrangThaiThanhToan = order.TrangThaiThanhToan,
            GhiChu = order.GhiChu,
            NgayTao = order.NgayTao,
            NgayCapNhat = order.NgayCapNhat,
            NgayThanhToanThanhCong = order.NgayThanhToanThanhCong,
            NgayHuyDon = order.NgayHuyDon,
            LyDoHuyDon = order.LyDoHuyDon,
            PhuongThucNhanHang = order.PhuongThucNhanHang,
            TrangThaiVanChuyen = order.TrangThaiVanChuyen,
            LoaiDonHang = order.LoaiDonHang,
            TienDatCoc = order.TienDatCoc,
            SoTienConLai = order.SoTienConLai,
            NgayHenNhanXe = order.NgayHenNhanXe,
            GhiChuGiaoNhan = order.GhiChuGiaoNhan,
            CheckoutHetHanLuc = activeHoldExpiry,
            PhuongThucThanhToan = order.Payments
                .OrderByDescending(p => p.NgayTao)
                .ThenByDescending(p => p.MaThanhToan)
                .Select(p => p.PhuongThuc)
                .FirstOrDefault(),
            Items = order.Items.Select(MapOrderItem).ToList(),
            Vouchers = order.Vouchers.Select(MapOrderVoucher).ToList(),
            LichSu = order.Histories
                .OrderBy(h => h.ThoiGian)
                .ThenBy(h => h.MaLichSuDonHang)
                .Select(MapOrderHistory)
                .ToList(),
            DanhSachThanhToan = order.Payments
                .OrderByDescending(p => p.NgayTao)
                .ThenByDescending(p => p.MaThanhToan)
                .Select(MapPayment)
                .ToList(),
            TraGop = order.InstallmentPlan is null ? null : MapInstallmentPlan(order.InstallmentPlan),
            YeuCauHoanTien = order.RefundRequests
                .OrderByDescending(r => r.NgayTao)
                .Select(MapRefundRequest)
                .ToList()
        };
    }

    private static RefundRequestDto MapRefundRequest(RefundRequest r)
    {
        return new RefundRequestDto
        {
            MaYeuCauHoanTien = r.MaYeuCauHoanTien,
            MaDonHang = r.MaDonHang,
            SoTien = r.SoTien,
            TenNganHang = r.TenNganHang,
            SoTaiKhoan = r.SoTaiKhoan,
            ChuTaiKhoan = r.ChuTaiKhoan,
            LyDo = r.LyDo,
            TrangThai = r.TrangThai,
            NgayTao = r.NgayTao,
            NgayHoanTat = r.NgayHoanTat,
            GhiChuAdmin = r.GhiChuAdmin,
            MaGiaoDichHoan = r.MaGiaoDichHoan
        };
    }

    private static PaymentDto MapPayment(Payment p)
    {
        return new PaymentDto
        {
            MaThanhToan = p.MaThanhToan,
            MaThanhToanKinhDoanh = p.MaThanhToanKinhDoanh,
            MaDonHang = p.MaDonHang,
            SoTien = p.SoTien,
            PhuongThuc = p.PhuongThuc,
            TrangThai = p.TrangThai,
            LoaiThanhToan = p.LoaiThanhToan,
            MaGiaoDich = p.MaGiaoDich,
            NoiDungChuyenKhoan = p.NoiDungChuyenKhoan,
            DaThanhToanLuc = p.DaThanhToanLuc,
            NgayTao = p.NgayTao
        };
    }

    private static InstallmentPlanDto MapInstallmentPlan(InstallmentPlan plan)
    {
        return new InstallmentPlanDto
        {
            MaHoSoTraGop = plan.MaHoSoTraGop,
            MaDonHang = plan.MaDonHang,
            TienTraTruoc = plan.TienTraTruoc,
            SoTienGoc = plan.SoTienGoc,
            SoKy = plan.SoKy,
            LaiSuatNam = plan.LaiSuatNam,
            TongTienLai = plan.TongTienLai,
            TongPhaiTra = plan.TongPhaiTra,
            TrangThai = plan.TrangThai,
            HoTenNguoiVay = plan.HoTenNguoiVay,
            SoCCCD = plan.SoCCCD,
            NgayCapCCCD = plan.NgayCapCCCD,
            NoiCapCCCD = plan.NoiCapCCCD,
            NgaySinh = plan.NgaySinh,
            SoDienThoai = plan.SoDienThoai,
            DiaChiThuongTru = plan.DiaChiThuongTru,
            NgheNghiep = plan.NgheNghiep,
            TenCongTy = plan.TenCongTy,
            ThoiGianLamViecThang = plan.ThoiGianLamViecThang,
            ThuNhapHangThang = plan.ThuNhapHangThang
        };
    }

    private static OrderItemDto MapOrderItem(OrderItem item)
    {
        return new OrderItemDto
        {
            MaChiTietDonHang = item.MaChiTietDonHang,
            MaSanPham = item.MaSanPham,
            MaBienSanPham = item.MaBienSanPham,
            TenSanPhamSnapshot = item.TenSanPhamSnapshot,
            SKUSnapshot = item.SKUSnapshot,
            DonGia = item.DonGia,
            SoLuong = item.SoLuong,
            ThanhTien = item.DonGia * item.SoLuong
        };
    }

    private static OrderVoucherDto MapOrderVoucher(OrderVoucher voucher)
    {
        return new OrderVoucherDto
        {
            MaVoucher = voucher.MaVoucher,
            MaVoucherCodeSnapshot = voucher.MaVoucherCodeSnapshot,
            SoTienGiam = voucher.SoTienGiam,
            LoaiGiamGiaSnapshot = voucher.LoaiGiamGiaSnapshot,
            GiaTriGiamSnapshot = voucher.GiaTriGiamSnapshot
        };
    }

    private static OrderHistoryDto MapOrderHistory(OrderHistory history)
    {
        return new OrderHistoryDto
        {
            MaLichSuDonHang = history.MaLichSuDonHang,
            MaDonHang = history.MaDonHang,
            LoaiSuKien = history.LoaiSuKien,
            GiaTriCu = history.GiaTriCu,
            GiaTriMoi = history.GiaTriMoi,
            GhiChu = history.GhiChu,
            MaNguoiThucHien = history.MaNguoiThucHien,
            ThoiGian = history.ThoiGian
        };
    }

    // Giá nằm ở biến thể (BIENSANPHAM): GiaKhuyenMai ?? GiaGoc.
    private static decimal GetUnitPrice(ProductVariant? variant)
    {
        if (variant is null)
        {
            throw new BusinessException("Khong xac dinh duoc gia san pham (thieu bien the).");
        }

        return variant.GiaKhuyenMai ?? variant.GiaGoc;
    }

    private static string NormalizeAllowedValue(string value, HashSet<string> allowedValues)
    {
        var match = allowedValues.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null)
        {
            throw new BusinessException("Gia tri khong hop le.");
        }

        return match;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string GenerateOrderCode()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..24];
    }

    private static string AppendNote(string? current, string note)
    {
        return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
    }

    private sealed record VoucherDiscountResult(decimal Amount, bool IsFreeShipping)
    {
        public static VoucherDiscountResult Empty { get; } = new(0, false);
    }
}
