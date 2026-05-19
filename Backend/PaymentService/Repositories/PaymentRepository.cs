using System.Data;
using PaymentService.Data;
using PaymentService.DTOs.Payments;
using PaymentService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace PaymentService.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public PaymentRepository(PaymentDbContext dbContext)
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

    public async Task<Order?> GetOrderByIdAsync(int maDonHang)
    {
        return await _dbContext.Orders
            .Include(o => o.Payments)
            .Include(o => o.InventoryHolds)
            .FirstOrDefaultAsync(o => o.MaDonHang == maDonHang);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int maThanhToan)
    {
        return await _dbContext.Payments
            .Include(p => p.Order)
            .ThenInclude(o => o!.Payments)
            .Include(p => p.Order)
            .ThenInclude(o => o!.InventoryHolds)
            .FirstOrDefaultAsync(p => p.MaThanhToan == maThanhToan);
    }

    public async Task<List<Payment>> GetPaymentsAsync(PaymentSearchDto search, int? maNguoiDung)
    {
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = search.PageSize <= 0 ? 20 : Math.Min(search.PageSize, 100);

        return await ApplyPaymentSearch(search, maNguoiDung)
            .OrderByDescending(p => p.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountPaymentsAsync(PaymentSearchDto search, int? maNguoiDung)
    {
        return await ApplyPaymentSearch(search, maNguoiDung).CountAsync();
    }

    public async Task<Product?> GetProductAsync(int maSanPham)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(p => p.MaSanPham == maSanPham);
    }

    public async Task<ProductVariant?> GetVariantAsync(int maBienSanPham)
    {
        return await _dbContext.ProductVariants.FirstOrDefaultAsync(v => v.MaBienSanPham == maBienSanPham);
    }

    public async Task AddPaymentAsync(Payment payment)
    {
        await _dbContext.Payments.AddAsync(payment);
    }

    public async Task CleanupExpiredInventoryHoldsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredHolds = await _dbContext.InventoryHolds
            .Where(h => h.TrangThai == "Active" && h.HetHanLuc <= now)
            .ToListAsync();

        if (expiredHolds.Count == 0)
        {
            return;
        }

        var affectedOrderIds = expiredHolds
            .Select(h => h.MaDonHang)
            .Distinct()
            .ToList();

        foreach (var hold in expiredHolds)
        {
            hold.TrangThai = "Expired";
            hold.NgayCapNhat = now;
            hold.GhiChu = AppendNote(hold.GhiChu, "Tu dong het han giu cho");
        }

        var orders = await _dbContext.Orders
            .Include(o => o.InventoryHolds)
            .Where(o =>
                affectedOrderIds.Contains(o.MaDonHang) &&
                (o.TrangThaiDonHang == "Pending" ||
                 o.TrangThaiDonHang == "Checkout" ||
                 o.TrangThaiDonHang == "AwaitingPayment"))
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

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private IQueryable<Payment> ApplyPaymentSearch(PaymentSearchDto search, int? maNguoiDung)
    {
        var query = _dbContext.Payments
            .Include(p => p.Order)
            .AsQueryable();

        if (maNguoiDung.HasValue)
        {
            query = query.Where(p => p.Order != null && p.Order.MaNguoiDung == maNguoiDung.Value);
        }
        else if (search.MaNguoiDung.HasValue)
        {
            query = query.Where(p => p.Order != null && p.Order.MaNguoiDung == search.MaNguoiDung.Value);
        }

        if (search.MaDonHang.HasValue)
        {
            query = query.Where(p => p.MaDonHang == search.MaDonHang.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.TrangThai))
        {
            query = query.Where(p => p.TrangThai == search.TrangThai);
        }

        if (!string.IsNullOrWhiteSpace(search.PhuongThuc))
        {
            query = query.Where(p => p.PhuongThuc == search.PhuongThuc);
        }

        if (!string.IsNullOrWhiteSpace(search.LoaiThanhToan))
        {
            query = query.Where(p => p.LoaiThanhToan == search.LoaiThanhToan);
        }

        if (search.TuNgay.HasValue)
        {
            query = query.Where(p => p.NgayTao >= search.TuNgay.Value);
        }

        if (search.DenNgay.HasValue)
        {
            query = query.Where(p => p.NgayTao <= search.DenNgay.Value);
        }

        return query;
    }

    private static string AppendNote(string? current, string note)
    {
        return string.IsNullOrWhiteSpace(current) ? note : $"{current} | {note}";
    }
}
