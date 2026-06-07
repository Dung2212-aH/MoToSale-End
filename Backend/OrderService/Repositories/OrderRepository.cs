using System.Data;
using OrderService.Data;
using OrderService.DTOs.Orders;
using OrderService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace OrderService.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        return await _dbContext.Database.BeginTransactionAsync(isolationLevel);
    }

    public async Task<User?> GetUserAsync(int maNguoiDung)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.MaNguoiDung == maNguoiDung);
    }

    public async Task<UserAddress?> GetUserAddressAsync(int maNguoiDung, int maDiaChi)
    {
        return await _dbContext.UserAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.MaNguoiDung == maNguoiDung && a.MaDiaChi == maDiaChi);
    }

    public async Task<Product?> GetProductAsync(int maSanPham)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == maSanPham);
    }

    public async Task<ProductVariant?> GetVariantAsync(int maBienSanPham)
    {
        return await _dbContext.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.MaBienSanPham == maBienSanPham);
    }

    public async Task<bool> ProductHasVariantsAsync(int maSanPham)
    {
        return await _dbContext.ProductVariants.AnyAsync(v => v.MaSanPham == maSanPham);
    }

    public async Task<Cart?> GetActiveCartByUserIdAsync(int maNguoiDung)
    {
        return await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .Include(c => c.Items)
            .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.MaNguoiDung == maNguoiDung && c.TrangThai == "Active");
    }

    public async Task<CartItem?> GetCartItemForUserAsync(int maNguoiDung, int maChiTietGioHang)
    {
        return await _dbContext.CartItems
            .Include(i => i.Cart)
            .Include(i => i.Product)
            .Include(i => i.Variant)
            .FirstOrDefaultAsync(i =>
                i.MaChiTietGioHang == maChiTietGioHang &&
                i.Cart != null &&
                i.Cart.MaNguoiDung == maNguoiDung &&
                i.Cart.TrangThai == "Active");
    }

    public async Task AddCartAsync(Cart cart)
    {
        await _dbContext.Carts.AddAsync(cart);
    }

    public async Task AddCartItemAsync(CartItem cartItem)
    {
        await _dbContext.CartItems.AddAsync(cartItem);
    }

    public void RemoveCartItem(CartItem cartItem)
    {
        _dbContext.CartItems.Remove(cartItem);
    }

    public void RemoveCartItems(IEnumerable<CartItem> cartItems)
    {
        _dbContext.CartItems.RemoveRange(cartItems);
    }

    public async Task<int> GetAvailableStockAsync(int maSanPham, int? maBienSanPham)
    {
        var now = DateTime.UtcNow;
        var held = await _dbContext.InventoryHolds
            .Where(h =>
                h.MaSanPham == maSanPham &&
                h.TrangThai == "Active" &&
                h.HetHanLuc > now &&
                (maBienSanPham.HasValue
                    ? h.MaBienSanPham == maBienSanPham.Value
                    : h.MaBienSanPham == null))
            .SumAsync(h => (int?)h.SoLuong) ?? 0;

        var stock = maBienSanPham.HasValue
            ? await _dbContext.ProductVariants
                .Where(v => v.MaBienSanPham == maBienSanPham.Value)
                .Select(v => v.SoLuongTon ?? 0)
                .FirstOrDefaultAsync()
            : await _dbContext.Products
                .Where(p => p.MaSanPham == maSanPham)
                .Select(p => p.SoLuongTon)
                .FirstOrDefaultAsync();

        return stock - held;
    }

    public async Task CleanupExpiredInventoryHoldsAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await _dbContext.InventoryHolds
            .Where(h => h.TrangThai == "Active" && h.HetHanLuc <= now)
            .ToListAsync();
        var expiredOrderIds = expired
            .Select(h => h.MaDonHang)
            .Distinct()
            .ToList();

        foreach (var hold in expired)
        {
            hold.TrangThai = "Expired";
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, "Tu dong het han giu cho");
        }

        if (expiredOrderIds.Count == 0)
        {
            return;
        }

        var orders = await _dbContext.Orders
            .Include(o => o.InventoryHolds)
            .Where(o =>
                expiredOrderIds.Contains(o.MaDonHang) &&
                o.TrangThaiDonHang == "AwaitingPayment")
            .ToListAsync();

        foreach (var order in orders)
        {
            var hasActiveHold = order.InventoryHolds.Any(h => h.TrangThai == "Active" && h.HetHanLuc > now);
            if (hasActiveHold)
            {
                continue;
            }

            order.TrangThaiDonHang = "Cancelled";
            order.TrangThaiThanhToan = "Cancelled";
            order.NgayHuyDon ??= now;
            order.LyDoHuyDon ??= "Het thoi gian thanh toan";
            order.NgayCapNhat = now;
        }
    }

    public async Task AddOrderAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public async Task<Order?> GetOrderByIdAsync(int maDonHang)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.InventoryHolds)
            .Include(o => o.Vouchers)
            .Include(o => o.Histories)
            .Include(o => o.Payments)
            .Include(o => o.InstallmentPlan!)
            .ThenInclude(p => p.Terms)
            .Include(o => o.RefundRequests)
            .FirstOrDefaultAsync(o => o.MaDonHang == maDonHang);
    }

    public async Task<List<Order>> GetOrdersAsync(OrderSearchDto search, int? maNguoiDung, bool hideAwaitingPayment = false)
    {
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);

        return await ApplyOrderSearch(search, maNguoiDung, hideAwaitingPayment)
            .OrderByDescending(o => o.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountOrdersAsync(OrderSearchDto search, int? maNguoiDung, bool hideAwaitingPayment = false)
    {
        return await ApplyOrderSearch(search, maNguoiDung, hideAwaitingPayment).CountAsync();
    }

    public async Task<VoucherValidationResult?> ValidateVoucherAsync(
        int maNguoiDung,
        int maGioHang,
        string maVoucherCode,
        decimal phiVanChuyen)
    {
        var results = await _dbContext.VoucherValidationResults
            .FromSqlInterpolated(
                $"EXEC dbo.sp_Voucher_KiemTraTruocKhiTaoDon @MaNguoiDung={maNguoiDung}, @MaGioHang={maGioHang}, @MaVoucherCode={maVoucherCode}, @PhiVanChuyen={phiVanChuyen}")
            .AsNoTracking()
            .ToListAsync();

        return results.FirstOrDefault();
    }

    public async Task<bool> UserHasSavedVoucherAsync(int maNguoiDung, string maVoucherCode)
    {
        return await _dbContext.VoucherUsers
            .AsNoTracking()
            .AnyAsync(vu =>
                vu.MaNguoiDung == maNguoiDung &&
                vu.MaVoucherCodeSnapshot == maVoucherCode &&
                vu.TrangThai == "Saved");
    }

    public async Task RecordVoucherUseAsync(int maNguoiDung, int maDonHang, string maVoucherCode, decimal soTienGiam)
    {
        // SP ghi nhận sử dụng voucher (canonical): chèn 1 dòng VOUCHER_NGUOIDUNG (TrangThai='Used'),
        // upsert DONHANG_VOUCHER và tăng VOUCHER.SoLanDaDung (idempotent theo từng đơn).
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.sp_Voucher_GhiNhanSuDung @MaNguoiDung={maNguoiDung}, @MaDonHang={maDonHang}, @MaVoucherCode={maVoucherCode}, @SoTienGiam={soTienGiam}");

        // SP đã tạo bản ghi 'Used'. Xóa bản 'Saved' cũ (đã được thay thế) để nó biến mất khỏi
        // danh sách voucher đã lưu của khách, MÀ KHÔNG tạo thêm dòng 'Used' thứ hai.
        // (Trước đây ở đây UPDATE bản 'Saved' -> 'Used' gây ghi nhận trùng: mỗi đơn dùng voucher
        //  sinh 2 dòng 'Used' -> đếm sai giới hạn mỗi người và làm lệch SoLanDaDung khi hủy đơn.)
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM dbo.VOUCHER_NGUOIDUNG
               WHERE MaNguoiDung = {maNguoiDung}
                 AND MaVoucherCodeSnapshot = {maVoucherCode}
                 AND TrangThai = 'Saved'");
    }

    public async Task CancelVoucherUseAsync(int maDonHang)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.sp_Voucher_HuySuDungTheoDon @MaDonHang={maDonHang}");
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<Order> ApplyOrderSearch(OrderSearchDto search, int? maNguoiDung, bool hideAwaitingPayment = false)
    {
        var query = _dbContext.Orders
            .Include(o => o.Items)
            .Include(o => o.Vouchers)
            .AsQueryable();

        if (maNguoiDung.HasValue)
        {
            query = query.Where(o => o.MaNguoiDung == maNguoiDung.Value);
        }
        else if (search.MaNguoiDung.HasValue)
        {
            query = query.Where(o => o.MaNguoiDung == search.MaNguoiDung.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.TrangThaiDonHang))
        {
            query = query.Where(o => o.TrangThaiDonHang == search.TrangThaiDonHang);
        }
        else if (hideAwaitingPayment)
        {
            // Customer-facing "My orders" list hides orders that still need to be paid for —
            // they're parked on /checkout/payment instead and only show up here after admin confirms.
            query = query.Where(o => o.TrangThaiDonHang != "AwaitingPayment");
        }

        if (!string.IsNullOrWhiteSpace(search.TrangThaiThanhToan))
        {
            query = query.Where(o => o.TrangThaiThanhToan == search.TrangThaiThanhToan);
        }

        if (!string.IsNullOrWhiteSpace(search.TrangThaiVanChuyen))
        {
            query = query.Where(o => o.TrangThaiVanChuyen == search.TrangThaiVanChuyen);
        }

        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            var keyword = search.Keyword.Trim().ToLower();
            query = query.Where(o =>
                o.MaDonHang.ToString().Contains(keyword) ||
                o.MaDonHangKinhDoanh.ToLower().Contains(keyword) ||
                o.HoTenNhanHang.ToLower().Contains(keyword) ||
                o.SoDienThoaiNhanHang.Contains(keyword) ||
                (o.EmailNhanHang != null && o.EmailNhanHang.ToLower().Contains(keyword)));
        }

        if (search.TuNgay.HasValue)
        {
            query = query.Where(o => o.NgayTao >= search.TuNgay.Value);
        }

        if (search.DenNgay.HasValue)
        {
            query = query.Where(o => o.NgayTao <= search.DenNgay.Value);
        }

        return query;
    }

    private static string AppendNote(string? current, string note)
    {
        return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
    }
}
