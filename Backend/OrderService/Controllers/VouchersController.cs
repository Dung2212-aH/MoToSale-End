using OrderService.Data;
using OrderService.DTOs.Vouchers;
using OrderService.Entities;
using OrderService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers;

[ApiController]
[Route("api/vouchers")]
public class VouchersController : ControllerBase
{
    private readonly OrderDbContext _dbContext;
    private readonly IAuditLogService _auditLog;

    public VouchersController(OrderDbContext dbContext, IAuditLogService auditLog)
    {
        _dbContext = dbContext;
        _auditLog = auditLog;
    }

    /// <summary>
    /// Get all vouchers (admin/staff listing — includes usage counters and inactive vouchers)
    /// </summary>
    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> GetVouchers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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
    public async Task<IActionResult> GetVoucherById(int id)
    {
        var voucher = await _dbContext.Vouchers.AsNoTracking().FirstOrDefaultAsync(v => v.MaVoucher == id);
        return voucher is null ? NotFound(new { message = "Khong tim thay voucher." }) : Ok(await MapVoucherAsync(voucher));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost]
    public async Task<IActionResult> CreateVoucher(VoucherRequest request)
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

        var scope = NormalizeScope(request.Scope);
        var targetValidation = ValidateVoucherTargets(scope, request);
        if (targetValidation is not null)
        {
            return BadRequest(new { message = targetValidation });
        }
        var targetExistenceError = await ValidateVoucherTargetExistenceAsync(scope, request);
        if (targetExistenceError is not null)
        {
            return BadRequest(new { message = targetExistenceError });
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
            PhamViApDung = scope,
            ApDungLoaiDonHang = TrimToNull(request.OrderType)
        };

        _dbContext.Vouchers.Add(voucher);
        await _dbContext.SaveChangesAsync();
        await SaveVoucherTargetsAsync(voucher.MaVoucher, voucher.PhamViApDung, request);
        await _auditLog.WriteAsync(this, "Voucher", voucher.MaVoucher.ToString(), "Create", null, new { voucher.MaVoucher, voucher.MaVoucherCode, voucher.LoaiGiamGia, voucher.GiaTriGiam, voucher.PhamViApDung, voucher.DangHoatDong });

        return CreatedAtAction(nameof(GetVoucherById), new { id = voucher.MaVoucher }, await MapVoucherAsync(voucher));
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVoucher(int id, VoucherRequest request)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.MaVoucher == id);
        if (voucher is null)
        {
            return NotFound(new { message = "Khong tim thay voucher." });
        }
        var oldValue = new { voucher.MaVoucherCode, voucher.LoaiGiamGia, voucher.GiaTriGiam, voucher.GiaTriDonToiThieu, voucher.GiaTriGiamToiDa, voucher.NgayBatDau, voucher.NgayKetThuc, voucher.GioiHanSuDung, voucher.DangHoatDong, voucher.MoTa, voucher.PhamViApDung, voucher.ApDungLoaiDonHang };

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
        var scope = NormalizeScope(request.Scope);
        var targetValidation = ValidateVoucherTargets(scope, request);
        if (targetValidation is not null)
        {
            return BadRequest(new { message = targetValidation });
        }
        var targetExistenceError = await ValidateVoucherTargetExistenceAsync(scope, request);
        if (targetExistenceError is not null)
        {
            return BadRequest(new { message = targetExistenceError });
        }

        voucher.PhamViApDung = scope;
        voucher.ApDungLoaiDonHang = TrimToNull(request.OrderType);
        voucher.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await SaveVoucherTargetsAsync(voucher.MaVoucher, voucher.PhamViApDung, request);
        await _auditLog.WriteAsync(this, "Voucher", voucher.MaVoucher.ToString(), "Update", oldValue, new { voucher.MaVoucherCode, voucher.LoaiGiamGia, voucher.GiaTriGiam, voucher.GiaTriDonToiThieu, voucher.GiaTriGiamToiDa, voucher.NgayBatDau, voucher.NgayKetThuc, voucher.GioiHanSuDung, voucher.DangHoatDong, voucher.MoTa, voucher.PhamViApDung, voucher.ApDungLoaiDonHang });

        return Ok(await MapVoucherAsync(voucher));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVoucher(int id)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.MaVoucher == id);
        if (voucher is null)
        {
            return NotFound(new { message = "Khong tim thay voucher." });
        }
        var oldValue = new { voucher.MaVoucher, voucher.MaVoucherCode, voucher.LoaiGiamGia, voucher.GiaTriGiam, voucher.PhamViApDung, voucher.DangHoatDong };

        _dbContext.Vouchers.Remove(voucher);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "Voucher", id.ToString(), "Delete", oldValue, null);
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

            var hasSavedVoucher = await _dbContext.VoucherUsers
                .AsNoTracking()
                .AnyAsync(vu =>
                    vu.MaNguoiDung == userId &&
                    vu.MaVoucherCodeSnapshot == request.Code &&
                    vu.TrangThai == "Saved");

            if (!hasSavedVoucher)
            {
                return BadRequest(new { valid = false, message = "Ban chua nhan voucher nay." });
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
    private async Task<object> MapVoucherAsync(Voucher v)
    {
        var productIds = await LoadVoucherTargetIdsAsync("VOUCHER_SANPHAM", "MaSanPham", v.MaVoucher);
        var categoryIds = await LoadVoucherTargetIdsAsync("VOUCHER_DANHMUC", "MaDanhMuc", v.MaVoucher);
        var brandIds = await LoadVoucherTargetIdsAsync("VOUCHER_HANGXE", "MaHangXe", v.MaVoucher);

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
            productIds,
            categoryIds,
            brandIds,
            status = v.DangHoatDong ? "Active" : "Inactive",
            dangHoatDong = v.DangHoatDong,
            ngayTao = v.NgayTao
        };
    }

    private async Task<List<int>> LoadVoucherTargetIdsAsync(string tableName, string columnName, int voucherId)
    {
        var safeTable = tableName switch
        {
            "VOUCHER_SANPHAM" => "VOUCHER_SANPHAM",
            "VOUCHER_DANHMUC" => "VOUCHER_DANHMUC",
            "VOUCHER_HANGXE" => "VOUCHER_HANGXE",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };

        var safeColumn = columnName switch
        {
            "MaSanPham" => "MaSanPham",
            "MaDanhMuc" => "MaDanhMuc",
            "MaHangXe" => "MaHangXe",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName))
        };

        return await _dbContext.Database
            .SqlQueryRaw<int>($"SELECT {safeColumn} AS Value FROM dbo.{safeTable} WHERE MaVoucher = {{0}}", voucherId)
            .ToListAsync();
    }

    private static string? ValidateVoucherTargets(string scope, VoucherRequest request)
    {
        var productIds = CleanIds(request.ProductIds);
        var categoryIds = CleanIds(request.CategoryIds);
        var brandIds = CleanIds(request.BrandIds);

        if (scope == "Product" && productIds.Count == 0)
        {
            return "Vui long chon it nhat mot san pham ap dung.";
        }

        if (scope == "Category" && categoryIds.Count == 0)
        {
            return "Vui long chon it nhat mot danh muc ap dung.";
        }

        if (scope == "Brand" && brandIds.Count == 0)
        {
            return "Vui long chon it nhat mot hang xe ap dung.";
        }

        return null;
    }

    private async Task SaveVoucherTargetsAsync(int voucherId, string scope, VoucherRequest request)
    {
        var productIds = CleanIds(request.ProductIds);
        var categoryIds = CleanIds(request.CategoryIds);
        var brandIds = CleanIds(request.BrandIds);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.VOUCHER_SANPHAM WHERE MaVoucher = {voucherId}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.VOUCHER_DANHMUC WHERE MaVoucher = {voucherId}");
        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM dbo.VOUCHER_HANGXE WHERE MaVoucher = {voucherId}");

        if (scope == "Product")
        {
            foreach (var productId in productIds)
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO dbo.VOUCHER_SANPHAM (MaVoucher, MaSanPham) VALUES ({voucherId}, {productId})");
            }
        }
        else if (scope == "Category")
        {
            foreach (var categoryId in categoryIds)
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO dbo.VOUCHER_DANHMUC (MaVoucher, MaDanhMuc) VALUES ({voucherId}, {categoryId})");
            }
        }
        else if (scope == "Brand")
        {
            foreach (var brandId in brandIds)
            {
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO dbo.VOUCHER_HANGXE (MaVoucher, MaHangXe) VALUES ({voucherId}, {brandId})");
            }
        }

        await transaction.CommitAsync();
    }

    private async Task<string?> ValidateVoucherTargetExistenceAsync(string scope, VoucherRequest request)
    {
        if (scope == "Product")
        {
            var ids = CleanIds(request.ProductIds);
            var count = await _dbContext.Products.CountAsync(p => ids.Contains(p.MaSanPham));
            return count == ids.Count ? null : "Danh sach san pham ap dung khong hop le.";
        }

        if (scope == "Category")
        {
            var ids = CleanIds(request.CategoryIds);
            var count = await CountExistingIdsAsync("DANHMUC", "MaDanhMuc", ids);
            return count == ids.Count ? null : "Danh sach danh muc ap dung khong hop le.";
        }

        if (scope == "Brand")
        {
            var ids = CleanIds(request.BrandIds);
            var count = await CountExistingIdsAsync("HANGXE", "MaHangXe", ids);
            return count == ids.Count ? null : "Danh sach hang xe ap dung khong hop le.";
        }

        return null;
    }

    private async Task<int> CountExistingIdsAsync(string tableName, string columnName, List<int> ids)
    {
        if (ids.Count == 0) return 0;

        var safeTable = tableName switch
        {
            "DANHMUC" => "DANHMUC",
            "HANGXE" => "HANGXE",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName))
        };

        var safeColumn = columnName switch
        {
            "MaDanhMuc" => "MaDanhMuc",
            "MaHangXe" => "MaHangXe",
            _ => throw new ArgumentOutOfRangeException(nameof(columnName))
        };

        var csv = string.Join(",", ids);
        return await _dbContext.Database
            .SqlQueryRaw<int>($"SELECT COUNT(*) AS Value FROM dbo.{safeTable} WHERE {safeColumn} IN ({csv})")
            .FirstAsync();
    }

    private static List<int> CleanIds(IEnumerable<int>? ids)
    {
        return ids?
            .Where(id => id > 0)
            .Distinct()
            .ToList() ?? new List<int>();
    }

    private static string NormalizeDiscountType(string? value)
    {
        if (value?.Equals("Amount", StringComparison.OrdinalIgnoreCase) == true ||
            value?.Equals("Fixed", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Amount";
        }

        if (value?.Equals("FreeShipping", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "FreeShipping";
        }

        return "Percent";
    }

    private static bool NormalizeStatus(string? value)
    {
        return !string.Equals(value, "Inactive", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeScope(string? value)
    {
        if (value?.Equals("Product", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Product";
        }

        if (value?.Equals("Category", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Category";
        }

        if (value?.Equals("Brand", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "Brand";
        }

        return "All";
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
    public List<int>? ProductIds { get; set; }
    public List<int>? CategoryIds { get; set; }
    public List<int>? BrandIds { get; set; }
}
