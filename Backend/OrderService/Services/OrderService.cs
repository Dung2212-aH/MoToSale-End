using System.Data;
using OrderService.DTOs.Cart;
using OrderService.DTOs.Common;
using OrderService.DTOs.Orders;
using OrderService.Entities;
using OrderService.Exceptions;
using OrderService.Repositories;

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
    private const string NotShippedStatus = "NotShipped";

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

    private static readonly HashSet<string> AllowedOrderStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Checkout",
        "AwaitingPayment",
        "Confirmed",
        "Processing",
        "Completed",
        "Cancelled"
    };

    private static readonly HashSet<string> CustomerCancelableStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Checkout",
        "AwaitingPayment"
    };

    private static readonly HashSet<string> AllowedShippingStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "NotShipped",
        "Preparing",
        "Shipping",
        "Delivered",
        "PickupReady",
        "PickedUp",
        "Cancelled"
    };

    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<CartDto> GetMyCartAsync(int maNguoiDung)
    {
        await EnsureActiveUserAsync(maNguoiDung);

        var cart = await _orderRepository.GetActiveCartByUserIdAsync(maNguoiDung);
        return cart is null ? EmptyCart(maNguoiDung) : MapCart(cart);
    }

    public async Task<CartDto> AddCartItemAsync(int maNguoiDung, AddCartItemRequest request)
    {
        await EnsureActiveUserAsync(maNguoiDung);
        var product = await ValidateProductAsync(request.MaSanPham);
        var variant = await ValidateVariantAsync(product.MaSanPham, request.MaBienSanPham);
        var unitPrice = GetUnitPrice(product, variant);

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
            i.MaBienSanPham == request.MaBienSanPham);

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
        cartItem.DonGia = GetUnitPrice(product, variant);
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

        if (request.MaShowroom.HasValue && !await _orderRepository.ShowroomExistsAsync(request.MaShowroom.Value))
        {
            throw new BusinessException("Showroom khong ton tai hoac da ngung hoat dong.");
        }

        var now = DateTime.UtcNow;
        await RefreshAndValidateCartItemsAsync(cart);

        var subtotal = cart.Items.Sum(i => i.DonGia * i.SoLuong);
        var voucherDiscount = await GetVoucherDiscountAsync(maNguoiDung, cart.MaGioHang, request.MaVoucherCode, request.PhiVanChuyen);
        var discount = Math.Min(voucherDiscount, subtotal + request.PhiVanChuyen);
        var total = subtotal + request.PhiVanChuyen - discount;
        var deposit = GetDepositAmount(request.LoaiDonHang, request.TienDatCoc, total);
        var remaining = GetRemainingAmount(request.LoaiDonHang, deposit, total);

        var order = new Order
        {
            MaDonHangKinhDoanh = GenerateOrderCode(),
            MaNguoiDung = maNguoiDung,
            MaShowroom = request.MaShowroom,
            HoTenNhanHang = request.HoTenNhanHang.Trim(),
            SoDienThoaiNhanHang = request.SoDienThoaiNhanHang.Trim(),
            EmailNhanHang = string.IsNullOrWhiteSpace(request.EmailNhanHang) ? null : request.EmailNhanHang.Trim().ToLowerInvariant(),
            DiaChiNhanHang = request.DiaChiNhanHang.Trim(),
            TongTienHang = subtotal,
            TienGiam = discount,
            PhiVanChuyen = request.PhiVanChuyen,
            TongThanhToan = total,
            TrangThaiDonHang = AwaitingPaymentStatus,
            TrangThaiThanhToan = total == 0 ? "Paid" : UnpaidStatus,
            GhiChu = TrimToNull(request.GhiChu),
            NgayTao = now,
            NgayCapNhat = now,
            MaGioHang = cart.MaGioHang,
            PhuongThucNhanHang = NormalizeAllowedValue(request.PhuongThucNhanHang, AllowedReceiveMethods),
            TrangThaiVanChuyen = NotShippedStatus,
            LoaiDonHang = NormalizeAllowedValue(request.LoaiDonHang, AllowedOrderTypes),
            TienDatCoc = deposit,
            SoTienConLai = remaining,
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
                HetHanLuc = now.AddMinutes(request.SoPhutGiuCho),
                NgayTao = now,
                NgayCapNhat = now,
                GhiChu = "Giu ton kho khi tao don hang"
            });
        }

        cart.TrangThai = CheckedOutCartStatus;
        cart.NgayCapNhat = now;

        await _orderRepository.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(request.MaVoucherCode) && discount > 0)
        {
            await _orderRepository.RecordVoucherUseAsync(maNguoiDung, order.MaDonHang, request.MaVoucherCode.Trim(), discount);
        }

        await transaction.CommitAsync();

        var createdOrder = await _orderRepository.GetOrderByIdAsync(order.MaDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang vua tao.");

        return MapOrder(createdOrder);
    }

    public async Task<PagedResultDto<OrderSummaryDto>> GetOrdersAsync(OrderSearchDto search, int currentUserId, bool canViewAll)
    {
        int? userFilter = canViewAll ? null : currentUserId;
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);

        var orders = await _orderRepository.GetOrdersAsync(search, userFilter);
        var totalItems = await _orderRepository.CountOrdersAsync(search, userFilter);

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

        var now = DateTime.UtcNow;
        order.TrangThaiDonHang = CancelledOrderStatus;
        order.TrangThaiVanChuyen = "Cancelled";
        order.NgayHuyDon = now;
        order.LyDoHuyDon = TrimToNull(request.LyDoHuyDon) ?? "Khach hang huy don";
        order.NgayCapNhat = now;

        await ReleaseInventoryForCancelledOrderAsync(order, now, "Huy don, nha giu cho", "Huy don, hoan ton kho");

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

    public async Task<OrderDto> UpdateOrderStatusAsync(int maDonHang, UpdateOrderStatusRequest request)
    {
        var status = NormalizeAllowedValue(request.TrangThaiDonHang, AllowedOrderStatuses);
        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        order.TrangThaiDonHang = status;
        order.NgayCapNhat = DateTime.UtcNow;

        if (status == CancelledOrderStatus)
        {
            var now = DateTime.UtcNow;
            order.NgayHuyDon ??= now;
            order.TrangThaiVanChuyen = "Cancelled";
            await ReleaseInventoryForCancelledOrderAsync(order, now, "Admin huy don", "Admin huy don, hoan ton kho");
        }

        await _orderRepository.SaveChangesAsync();

        if (status == CancelledOrderStatus && order.Vouchers.Any())
        {
            await _orderRepository.CancelVoucherUseAsync(order.MaDonHang);
        }

        return MapOrder(order);
    }

    public async Task<OrderDto> UpdateShippingStatusAsync(int maDonHang, UpdateShippingStatusRequest request)
    {
        var status = NormalizeAllowedValue(request.TrangThaiVanChuyen, AllowedShippingStatuses);
        var order = await _orderRepository.GetOrderByIdAsync(maDonHang)
            ?? throw new NotFoundException("Khong tim thay don hang.");

        order.TrangThaiVanChuyen = status;
        order.NgayHenNhanXe = request.NgayHenNhanXe;
        order.GhiChuGiaoNhan = TrimToNull(request.GhiChuGiaoNhan);
        order.NgayCapNhat = DateTime.UtcNow;

        if (status is "Delivered" or "PickedUp")
        {
            order.TrangThaiDonHang = "Completed";
        }

        await _orderRepository.SaveChangesAsync();
        return MapOrder(order);
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
            if (await _orderRepository.ProductHasVariantsAsync(maSanPham))
            {
                throw new BusinessException("Vui long chon phien ban/mau sac truoc khi them vao gio hang.");
            }

            return null;
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
            item.DonGia = GetUnitPrice(product, variant);
            item.NgayCapNhat = DateTime.UtcNow;
        }
    }

    private async Task<decimal> GetVoucherDiscountAsync(int maNguoiDung, int maGioHang, string? maVoucherCode, decimal phiVanChuyen)
    {
        if (string.IsNullOrWhiteSpace(maVoucherCode))
        {
            return 0;
        }

        var voucher = await _orderRepository.ValidateVoucherAsync(maNguoiDung, maGioHang, maVoucherCode.Trim(), phiVanChuyen)
            ?? throw new BusinessException("Khong kiem tra duoc voucher.");

        if (!voucher.HopLe)
        {
            throw new BusinessException(voucher.LyDoKhongHopLe ?? "Voucher khong hop le.");
        }

        return voucher.SoTienGiam;
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

            variant.SoLuongTon = (variant.SoLuongTon ?? 0) + group.Sum(h => h.SoLuong);
            variant.NgayCapNhat = now;
        }

        foreach (var group in order.InventoryHolds
            .Where(h => h.TrangThai == ConfirmedOrderStatus && !h.MaBienSanPham.HasValue)
            .GroupBy(h => h.MaSanPham))
        {
            var product = await _orderRepository.GetProductAsync(group.Key)
                ?? throw new BusinessException("San pham trong don hang khong ton tai.");

            product.SoLuongTon += group.Sum(h => h.SoLuong);
            product.NgayCapNhat = now;
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

        if (request.PhuongThucNhanHang.Equals("Pickup", StringComparison.OrdinalIgnoreCase) && !request.MaShowroom.HasValue)
        {
            throw new BusinessException("Don nhan tai showroom can co MaShowroom.");
        }
    }

    private static decimal GetDepositAmount(string orderType, decimal requestedDeposit, decimal total)
    {
        var normalizedType = NormalizeAllowedValue(orderType, AllowedOrderTypes);

        if (normalizedType == "FullPayment")
        {
            return 0;
        }

        if (normalizedType == "Deposit" && (requestedDeposit <= 0 || requestedDeposit >= total))
        {
            throw new BusinessException("Tien dat coc phai lon hon 0 va nho hon tong thanh toan.");
        }

        if (normalizedType == "Installment" && (requestedDeposit < 0 || requestedDeposit >= total))
        {
            throw new BusinessException("Tien tra truoc phai nho hon tong thanh toan.");
        }

        return requestedDeposit;
    }

    private static decimal GetRemainingAmount(string orderType, decimal deposit, decimal total)
    {
        var normalizedType = NormalizeAllowedValue(orderType, AllowedOrderTypes);
        return normalizedType == "FullPayment" ? 0 : total - deposit;
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

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.NgayTao)
            .Select(MapCartItem)
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

    private static CartItemDto MapCartItem(CartItem item)
    {
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
            AnhChinhUrl = item.Product?.AnhChinhUrl
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
            TongThanhToan = order.TongThanhToan,
            TrangThaiDonHang = order.TrangThaiDonHang,
            TrangThaiThanhToan = order.TrangThaiThanhToan,
            TrangThaiVanChuyen = order.TrangThaiVanChuyen,
            LoaiDonHang = order.LoaiDonHang,
            NgayTao = order.NgayTao
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
            MaShowroom = order.MaShowroom,
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
            Items = order.Items.Select(MapOrderItem).ToList(),
            Vouchers = order.Vouchers.Select(MapOrderVoucher).ToList()
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

    private static decimal GetUnitPrice(Product product, ProductVariant? variant)
    {
        return variant?.GiaGhiDe ?? product.GiaKhuyenMai ?? product.GiaGoc;
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
}
