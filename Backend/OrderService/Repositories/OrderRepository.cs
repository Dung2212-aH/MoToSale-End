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

    public async Task<bool> ShowroomExistsAsync(int maShowroom)
    {
        return await _dbContext.Showrooms.AnyAsync(s => s.MaShowroom == maShowroom && s.DangHoatDong);
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
            .FirstOrDefaultAsync(o => o.MaDonHang == maDonHang);
    }

    public async Task<List<Order>> GetOrdersAsync(OrderSearchDto search, int? maNguoiDung)
    {
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);

        return await ApplyOrderSearch(search, maNguoiDung)
            .OrderByDescending(o => o.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountOrdersAsync(OrderSearchDto search, int? maNguoiDung)
    {
        return await ApplyOrderSearch(search, maNguoiDung).CountAsync();
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

    public async Task RecordVoucherUseAsync(int maNguoiDung, int maDonHang, string maVoucherCode, decimal soTienGiam)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.sp_Voucher_GhiNhanSuDung @MaNguoiDung={maNguoiDung}, @MaDonHang={maDonHang}, @MaVoucherCode={maVoucherCode}, @SoTienGiam={soTienGiam}");
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

    private IQueryable<Order> ApplyOrderSearch(OrderSearchDto search, int? maNguoiDung)
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

        if (!string.IsNullOrWhiteSpace(search.TrangThaiThanhToan))
        {
            query = query.Where(o => o.TrangThaiThanhToan == search.TrangThaiThanhToan);
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
