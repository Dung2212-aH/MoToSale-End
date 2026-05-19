using OrderService.Data;
using OrderService.DTOs.Vouchers;
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

    [HttpGet]
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

    [HttpPost("applicable")]
    public Task<IActionResult> GetApplicableVouchers(ApplicableVoucherRequest request)
    {
        return GetActiveVouchers(request.OrderType);
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
}
