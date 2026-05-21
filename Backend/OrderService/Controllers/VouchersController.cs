using OrderService.Data;
using OrderService.DTOs.Vouchers;
using OrderService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers;

[ApiController]
[Route("api/vouchers")]
public class VouchersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;

    public VouchersController(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all active vouchers (public, for display/claim purposes)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _dbContext.Vouchers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(v => v.MaVoucherCode.ToLower().Contains(s) || (v.MoTa != null && v.MoTa.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new
            {
                id = v.MaVoucher,
                code = v.MaVoucherCode,
                discountType = v.LoaiGiamGia,
                discountValue = v.GiaTriGiam,
                minOrderValue = v.GiaTriDonToiThieu,
                maxDiscountValue = v.GiaTriGiamToiDa,
                description = v.MoTa,
                startsAt = v.NgayBatDau,
                endsAt = v.NgayKetThuc,
                scope = v.PhamViApDung,
                usageLimit = v.GioiHanSuDung,
                usedCount = v.SoLanDaDung,
                status = v.DangHoatDong ? "Active" : "Inactive",
                dangHoatDong = v.DangHoatDong,
                ngayTao = v.NgayTao
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var voucher = await _dbContext.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.MaVoucher == id);
        return voucher is null ? NotFound(new { message = "Khong tim thay voucher." }) : Ok(MapVoucher(voucher));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(VoucherRequest request)
    {
        var code = request.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "Ma voucher la bat buoc." });
        }

        if (await _dbContext.Vouchers.AnyAsync(v => v.MaVoucherCode == code))
        {
            return BadRequest(new { message = "Ma voucher da ton tai." });
        }

        var now = DateTime.UtcNow;
        var voucher = new Voucher
        {
            MaVoucherCode = code,
            LoaiGiamGia = NormalizeDiscountType(request.DiscountType),
            GiaTriGiam = request.DiscountValue,
            GiaTriDonToiThieu = request.MinOrderValue ?? 0,
            GiaTriGiamToiDa = request.MaxDiscountValue,
            NgayBatDau = request.StartDate ?? now,
            NgayKetThuc = request.EndDate ?? now.AddYears(1),
            GioiHanSuDung = request.UsageLimit,
            SoLanDaDung = 0,
            DangHoatDong = NormalizeStatus(request.Status),
            NgayTao = now,
            NgayCapNhat = now,
            MoTa = TrimToNull(request.Description),
            SoLanToiDaMoiNguoiDung = request.MaxUsagePerUser ?? 1,
            PhamViApDung = NormalizeScope(request.Scope),
            ApDungLoaiDonHang = TrimToNull(request.OrderType)
        };

        _dbContext.Vouchers.Add(voucher);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = voucher.MaVoucher }, MapVoucher(voucher));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VoucherRequest request)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.MaVoucher == id);
        if (voucher is null)
        {
            return NotFound(new { message = "Khong tim thay voucher." });
        }

        var code = request.Code?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { message = "Ma voucher la bat buoc." });
        }

        if (await _dbContext.Vouchers.AnyAsync(v => v.MaVoucher != id && v.MaVoucherCode == code))
        {
            return BadRequest(new { message = "Ma voucher da ton tai." });
        }

        voucher.MaVoucherCode = code;
        voucher.LoaiGiamGia = NormalizeDiscountType(request.DiscountType);
        voucher.GiaTriGiam = request.DiscountValue;
        voucher.GiaTriDonToiThieu = request.MinOrderValue ?? 0;
        voucher.GiaTriGiamToiDa = request.MaxDiscountValue;
        voucher.NgayBatDau = request.StartDate ?? voucher.NgayBatDau;
        voucher.NgayKetThuc = request.EndDate ?? voucher.NgayKetThuc;
        voucher.GioiHanSuDung = request.UsageLimit;
        voucher.DangHoatDong = NormalizeStatus(request.Status);
        voucher.MoTa = TrimToNull(request.Description);
        voucher.SoLanToiDaMoiNguoiDung = request.MaxUsagePerUser ?? voucher.SoLanToiDaMoiNguoiDung;
        voucher.PhamViApDung = NormalizeScope(request.Scope);
        voucher.ApDungLoaiDonHang = TrimToNull(request.OrderType);
        voucher.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(MapVoucher(voucher));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.MaVoucher == id);
        if (voucher is null)
        {
            return NotFound(new { message = "Khong tim thay voucher." });
        }

        _dbContext.Vouchers.Remove(voucher);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveVouchers([FromQuery] string? orderType = null)
    {
        var now = DateTime.UtcNow;
        var query = _dbContext.Vouchers
            .AsNoTracking()
            .Where(v =>
                v.DangHoatDong &&
                v.NgayBatDau <= now &&
                v.NgayKetThuc >= now &&
                (!v.GioiHanSuDung.HasValue || v.SoLanDaDung < v.GioiHanSuDung.Value));

        if (!string.IsNullOrWhiteSpace(orderType))
        {
            query = query.Where(v => v.ApDungLoaiDonHang == null || v.ApDungLoaiDonHang == orderType);
        }

        var vouchers = await query
            .OrderBy(v => v.GiaTriDonToiThieu)
            .ThenByDescending(v => v.GiaTriGiam)
            .Take(20)
            .Select(v => new
            {
                id = v.MaVoucher,
                code = v.MaVoucherCode,
                discountType = v.LoaiGiamGia,
                discountValue = v.GiaTriGiam,
                minOrderValue = v.GiaTriDonToiThieu,
                maxDiscountValue = v.GiaTriGiamToiDa,
                description = v.MoTa,
                startsAt = v.NgayBatDau,
                endsAt = v.NgayKetThuc,
                scope = v.PhamViApDung
            })
            .ToListAsync();

        return Ok(vouchers);
    }

    /// <summary>
    /// Save/claim a voucher for the current user
    /// </summary>
    [Authorize]
    [HttpPost("save")]
    public async Task<IActionResult> SaveVoucher([FromBody] SaveVoucherRequest request)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var now = DateTime.UtcNow;

            var voucher = await _dbContext.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v =>
                    v.MaVoucherCode == request.Code &&
                    v.DangHoatDong &&
                    v.NgayBatDau <= now &&
                    v.NgayKetThuc >= now &&
                    (!v.GioiHanSuDung.HasValue || v.SoLanDaDung < v.GioiHanSuDung.Value));

            if (voucher is null)
            {
                return BadRequest(new { success = false, message = "Voucher không tồn tại hoặc đã hết hạn." });
            }

            // Check if user already saved this voucher
            var alreadySaved = await _dbContext.VoucherUsers
                .AsNoTracking()
                .AnyAsync(vu =>
                    vu.MaNguoiDung == userId &&
                    vu.MaVoucher == voucher.MaVoucher &&
                    vu.TrangThai == "Saved");

            if (alreadySaved)
            {
                return BadRequest(new { success = false, message = "Bạn đã nhận voucher này rồi." });
            }

            // Check if user already used this voucher max times
            var usedCount = await _dbContext.VoucherUsers
                .AsNoTracking()
                .CountAsync(vu =>
                    vu.MaNguoiDung == userId &&
                    vu.MaVoucher == voucher.MaVoucher &&
                    vu.TrangThai == "Used");

            if (usedCount >= voucher.SoLanToiDaMoiNguoiDung)
            {
                return BadRequest(new { success = false, message = "Bạn đã sử dụng voucher này đủ số lần cho phép." });
            }

            var voucherUser = new VoucherUser
            {
                MaVoucher = voucher.MaVoucher,
                MaNguoiDung = userId,
                MaVoucherCodeSnapshot = voucher.MaVoucherCode,
                LoaiGiamGiaSnapshot = voucher.LoaiGiamGia,
                GiaTriGiamSnapshot = voucher.GiaTriGiam,
                SoTienGiam = 0,
                TrangThai = "Saved",
                NgayTao = DateTime.Now
            };

            _dbContext.VoucherUsers.Add(voucherUser);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã nhận voucher thành công." });
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }

    /// <summary>
    /// Get vouchers saved by the current user (not yet used)
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyVouchers()
    {
        var userId = this.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var vouchers = await _dbContext.VoucherUsers
            .AsNoTracking()
            .Include(vu => vu.Voucher)
            .Where(vu =>
                vu.MaNguoiDung == userId &&
                vu.TrangThai == "Saved" &&
                vu.Voucher != null &&
                vu.Voucher.DangHoatDong &&
                vu.Voucher.NgayKetThuc >= now)
            .Select(vu => new
            {
                id = vu.Voucher!.MaVoucher,
                code = vu.Voucher.MaVoucherCode,
                discountType = vu.Voucher.LoaiGiamGia,
                discountValue = vu.Voucher.GiaTriGiam,
                minOrderValue = vu.Voucher.GiaTriDonToiThieu,
                maxDiscountValue = vu.Voucher.GiaTriGiamToiDa,
                description = vu.Voucher.MoTa,
                startsAt = vu.Voucher.NgayBatDau,
                endsAt = vu.Voucher.NgayKetThuc,
                scope = vu.Voucher.PhamViApDung,
                savedAt = vu.NgayTao
            })
            .OrderByDescending(x => x.savedAt)
            .ToListAsync();

        return Ok(vouchers);
    }

    /// <summary>
    /// Get count of saved vouchers for the current user
    /// </summary>
    [Authorize]
    [HttpGet("my/count")]
    public async Task<IActionResult> GetMyVoucherCount()
    {
        var userId = this.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var count = await _dbContext.VoucherUsers
            .AsNoTracking()
            .Where(vu =>
                vu.MaNguoiDung == userId &&
                vu.TrangThai == "Saved" &&
                _dbContext.Vouchers.Any(v =>
                    v.MaVoucher == vu.MaVoucher &&
                    v.DangHoatDong &&
                    v.NgayKetThuc >= now))
            .CountAsync();

        return Ok(new { count });
    }

    /// <summary>
    /// Get applicable vouchers for checkout — only returns user's saved vouchers that match the cart
    /// </summary>
    [Authorize]
    [HttpPost("applicable")]
    public async Task<IActionResult> GetApplicableVouchers(ApplicableVoucherRequest request)
    {
        var userId = this.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var savedVouchers = await _dbContext.VoucherUsers
            .AsNoTracking()
            .Include(vu => vu.Voucher)
            .Where(vu =>
                vu.MaNguoiDung == userId &&
                vu.TrangThai == "Saved" &&
                vu.Voucher != null &&
                vu.Voucher.DangHoatDong &&
                vu.Voucher.NgayBatDau <= now &&
                vu.Voucher.NgayKetThuc >= now &&
                (!vu.Voucher.GioiHanSuDung.HasValue || vu.Voucher.SoLanDaDung < vu.Voucher.GioiHanSuDung.Value))
            .Where(vu => vu.Voucher!.ApDungLoaiDonHang == null || vu.Voucher.ApDungLoaiDonHang == request.OrderType)
            .Where(vu => vu.Voucher!.GiaTriDonToiThieu <= request.Subtotal)
            .Select(vu => new
            {
                id = vu.Voucher!.MaVoucher,
                code = vu.Voucher.MaVoucherCode,
                discountType = vu.Voucher.LoaiGiamGia,
                discountValue = vu.Voucher.GiaTriGiam,
                minOrderValue = vu.Voucher.GiaTriDonToiThieu,
                maxDiscountValue = vu.Voucher.GiaTriGiamToiDa,
                description = vu.Voucher.MoTa,
                startsAt = vu.Voucher.NgayBatDau,
                endsAt = vu.Voucher.NgayKetThuc,
                scope = vu.Voucher.PhamViApDung
            })
            .OrderBy(v => v.minOrderValue)
            .ThenByDescending(v => v.discountValue)
            .ToListAsync();

        // Filter by scope: check if voucher applies to products in cart
        var productIds = request.ProductIds ?? new List<int>();
        var categoryIds = request.CategoryIds ?? new List<int>();
        var brandIds = request.BrandIds ?? new List<int>();

        var filteredVouchers = new List<object>();

        foreach (var v in savedVouchers)
        {
            if (v.scope == "All")
            {
                filteredVouchers.Add(v);
                continue;
            }

            if (v.scope == "Product" && productIds.Count > 0)
            {
                var hasMatch = await _dbContext.Database
                    .SqlQueryRaw<int>($"SELECT MaSanPham AS Value FROM dbo.VOUCHER_SANPHAM WHERE MaVoucher = {v.id} AND MaSanPham IN ({string.Join(",", productIds)})")
                    .AnyAsync();
                if (hasMatch) filteredVouchers.Add(v);
            }
            else if (v.scope == "Category" && categoryIds.Count > 0)
            {
                var hasMatch = await _dbContext.Database
                    .SqlQueryRaw<int>($"SELECT MaDanhMuc AS Value FROM dbo.VOUCHER_DANHMUC WHERE MaVoucher = {v.id} AND MaDanhMuc IN ({string.Join(",", categoryIds)})")
                    .AnyAsync();
                if (hasMatch) filteredVouchers.Add(v);
            }
            else if (v.scope == "Brand" && brandIds.Count > 0)
            {
                var hasMatch = await _dbContext.Database
                    .SqlQueryRaw<int>($"SELECT MaHangXe AS Value FROM dbo.VOUCHER_HANGXE WHERE MaVoucher = {v.id} AND MaHangXe IN ({string.Join(",", brandIds)})")
                    .AnyAsync();
                if (hasMatch) filteredVouchers.Add(v);
            }
        }

        return Ok(filteredVouchers);
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateVoucher(ValidateVoucherRequest request)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            var cart = await _dbContext.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaNguoiDung == userId && c.TrangThai == "Active");

            if (cart is null)
            {
                return BadRequest(new { valid = false, message = "Gio hang dang trong." });
            }

            var results = await _dbContext.VoucherValidationResults
                .FromSqlInterpolated(
                    $"EXEC dbo.sp_Voucher_KiemTraTruocKhiTaoDon @MaNguoiDung={userId}, @MaGioHang={cart.MaGioHang}, @MaVoucherCode={request.Code}, @PhiVanChuyen={request.ShippingFee}")
                .AsNoTracking()
                .ToListAsync();

            var result = results.FirstOrDefault();
            if (result is null)
            {
                return BadRequest(new { valid = false, message = "Khong kiem tra duoc voucher." });
            }

            return Ok(new
            {
                valid = result.HopLe,
                message = result.LyDoKhongHopLe,
                discountAmount = result.SoTienGiam,
                voucher = new
                {
                    id = result.MaVoucher,
                    code = result.MaVoucherCode,
                    discountType = result.LoaiGiamGia,
                    scope = result.PhamViApDung
                }
            });
        }
        catch (Exception ex)
        {
            return this.ToErrorResult(ex);
        }
    }
    private static object MapVoucher(Voucher v)
    {
        return new
        {
            id = v.MaVoucher,
            code = v.MaVoucherCode,
            discountType = v.LoaiGiamGia,
            discountValue = v.GiaTriGiam,
            minOrderValue = v.GiaTriDonToiThieu,
            maxDiscountValue = v.GiaTriGiamToiDa,
            startDate = v.NgayBatDau,
            endDate = v.NgayKetThuc,
            description = v.MoTa,
            scope = v.PhamViApDung,
            usageLimit = v.GioiHanSuDung,
            usedCount = v.SoLanDaDung,
            status = v.DangHoatDong ? "Active" : "Inactive",
            dangHoatDong = v.DangHoatDong,
            ngayTao = v.NgayTao
        };
    }

    private static string NormalizeDiscountType(string? value)
    {
        return value?.Equals("Fixed", StringComparison.OrdinalIgnoreCase) == true ? "Amount" : "Percent";
    }

    private static bool NormalizeStatus(string? value)
    {
        return !string.Equals(value, "Inactive", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeScope(string? value)
    {
        var scope = string.IsNullOrWhiteSpace(value) ? "All" : value.Trim();
        return scope.Length > 20 ? scope[..20] : scope;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public class VoucherRequest
{
    public string? Code { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountValue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public string? Description { get; set; }
    public string? Scope { get; set; }
    public string? Status { get; set; }
    public int? MaxUsagePerUser { get; set; }
    public string? OrderType { get; set; }
}
