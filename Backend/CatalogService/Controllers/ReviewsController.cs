using CatalogService.Data;
using CatalogService.DTOs.ProductReviews;
using CatalogService.Entities;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private const string PendingReviewStatus = "Pending";
    private const string ApprovedReviewStatus = "Approved";
    private const string RejectedReviewStatus = "Rejected";
    private const string HiddenReviewStatus = "Hidden";

    private static readonly HashSet<string> AllowedReviewStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        PendingReviewStatus,
        ApprovedReviewStatus,
        RejectedReviewStatus,
        HiddenReviewStatus
    };

    // Trang thai hoan tat lay theo OrderService.
    private static readonly string[] CompletedOrderStatuses = { "Completed" };
    private static readonly string[] CompletedShippingStatuses = { "Delivered" };

    private readonly CatalogDbContext _dbContext;
    private readonly IImageStorageService _imageStorage;
    private readonly IAuditLogService _auditLog;

    public ReviewsController(CatalogDbContext dbContext, IImageStorageService imageStorage, IAuditLogService auditLog)
    {
        _dbContext = dbContext;
        _imageStorage = imageStorage;
        _auditLog = auditLog;
    }

    [HttpGet("~/api/products/{productId:int}/reviews")]
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _dbContext.ProductReviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.MaSanPham == productId && r.TrangThai == ApprovedReviewStatus)
            .OrderByDescending(r => r.NgayTao)
            .Select(r => new ProductReviewDto
            {
                MaDanhGia = r.MaDanhGia,
                MaSanPham = r.MaSanPham,
                MaNguoiDung = r.MaNguoiDung,
                TenNguoiDung = r.User == null ? null : r.User.HoTen,
                MaDonHang = r.MaDonHang,
                Diem = r.Diem,
                TieuDe = r.TieuDe,
                NoiDung = r.NoiDung,
                HinhAnhUrl = r.HinhAnhUrl,
                TrangThai = r.TrangThai == RejectedReviewStatus ? HiddenReviewStatus : r.TrangThai,
                NgayTao = r.NgayTao,
                NgayCapNhat = r.NgayCapNhat
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("~/api/products/{productId:int}/reviews/summary")]
    public async Task<IActionResult> GetProductReviewSummary(int productId)
    {
        var summary = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.MaSanPham == productId && r.TrangThai == ApprovedReviewStatus)
            .GroupBy(r => r.MaSanPham)
            .Select(g => new ProductReviewSummaryDto
            {
                MaSanPham = productId,
                TongDanhGia = g.Count(),
                DiemTrungBinh = g.Average(r => r.Diem)
            })
            .FirstOrDefaultAsync();

        return Ok(summary ?? new ProductReviewSummaryDto { MaSanPham = productId });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] int? productId = null, [FromQuery] string? status = null)
    {
        var query = _dbContext.ProductReviews.AsNoTracking();

        if (productId.HasValue)
        {
            query = query.Where(r => r.MaSanPham == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeReviewStatus(status);
            if (normalizedStatus is null)
            {
                return BadRequest(new { message = "Trang thai danh gia khong hop le." });
            }

            query = query.Where(r => r.TrangThai == normalizedStatus);
        }

        var reviews = await query
            .Include(r => r.User)
            .OrderByDescending(r => r.NgayTao)
            .Select(r => new ProductReviewDto
            {
                MaDanhGia = r.MaDanhGia,
                MaSanPham = r.MaSanPham,
                MaNguoiDung = r.MaNguoiDung,
                TenNguoiDung = r.User == null ? null : r.User.HoTen,
                MaDonHang = r.MaDonHang,
                Diem = r.Diem,
                TieuDe = r.TieuDe,
                NoiDung = r.NoiDung,
                HinhAnhUrl = r.HinhAnhUrl,
                TrangThai = r.TrangThai == RejectedReviewStatus ? HiddenReviewStatus : r.TrangThai,
                NgayTao = r.NgayTao,
                NgayCapNhat = r.NgayCapNhat
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [Authorize]
    [HttpGet("product/{productId:int}/me")]
    public async Task<IActionResult> GetMyProductReviewState(int productId)
    {
        var userId = this.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { message = "Token khong hop le." });
        }

        if (!await ProductExistsAsync(productId))
        {
            return NotFound(new { message = "San pham khong ton tai hoac da ngung hoat dong." });
        }

        var existingReview = await _dbContext.ProductReviews
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.MaSanPham == productId && r.MaNguoiDung == userId.Value);
        var eligibleOrderId = await GetEligibleOrderIdAsync(userId.Value, productId);
        var hasPurchased = eligibleOrderId.HasValue;

        return Ok(new ProductReviewMeDto
        {
            MaSanPham = productId,
            DaDangNhap = true,
            DaMua = hasPurchased,
            CoTheDanhGia = hasPurchased,
            MaDonHangDuDieuKien = eligibleOrderId,
            LyDo = hasPurchased ? null : "Chi khach hang da mua va hoan tat don hang moi duoc danh gia san pham.",
            DanhGiaCuaToi = existingReview is null ? null : ToReviewDto(existingReview)
        });
    }

    [HttpPost("~/api/products/{productId:int}/reviews")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateReview([FromRoute] int productId, [FromForm] ProductReviewCreateDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Token khong hop le." });
            }

            if (productId <= 0)
            {
                return BadRequest(new { message = "Ma san pham khong hop le." });
            }

            var requestedProductId = productId;

            var validationError = ValidateReviewContent(dto.Diem, dto.NoiDung);
            if (validationError is not null)
            {
                return BadRequest(new { message = validationError });
            }

            if (!await ProductExistsAsync(requestedProductId))
            {
                return NotFound(new { message = "San pham khong ton tai hoac da ngung hoat dong." });
            }

            var eligibleOrderId = dto.MaDonHang.HasValue
                ? await GetEligibleOrderIdAsync(userId.Value, requestedProductId, dto.MaDonHang.Value)
                : await GetEligibleOrderIdAsync(userId.Value, requestedProductId);

            if (!eligibleOrderId.HasValue)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Chi khach hang da mua va hoan tat don hang moi duoc danh gia san pham." });
            }

            var existingReview = await _dbContext.ProductReviews
                .FirstOrDefaultAsync(r => r.MaSanPham == requestedProductId && r.MaNguoiDung == userId.Value);

            if (existingReview is not null)
            {
                return Conflict(new { message = "Ban da danh gia san pham nay. Moi san pham chi duoc danh gia mot lan." });
            }

            var now = DateTime.UtcNow;
            var review = new ProductReview
            {
                MaSanPham = requestedProductId,
                MaNguoiDung = userId.Value,
                NgayTao = now
            };

            _dbContext.ProductReviews.Add(review);
            review.MaDonHang = eligibleOrderId;
            review.Diem = dto.Diem;
            review.TieuDe = TrimToNull(dto.TieuDe);
            review.NoiDung = TrimToNull(dto.NoiDung);
            review.TrangThai = PendingReviewStatus;
            review.NgayCapNhat = now;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                try
                {
                    review.HinhAnhUrl = await _imageStorage.SaveImageAsync(dto.Image, "reviews", HttpContext.RequestAborted);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            await _dbContext.SaveChangesAsync();

            var savedReview = await _dbContext.ProductReviews
                .AsNoTracking()
                .Include(r => r.User)
                .FirstAsync(r => r.MaDanhGia == review.MaDanhGia);

            return Ok(new { message = "Danh gia cua ban da duoc gui va dang cho duyet.", review = ToReviewDto(savedReview) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Da xay ra loi khi gui danh gia.", details = ex.Message });
        }
    }

    [Authorize]
    [HttpPatch("~/api/products/{productId:int}/reviews/me")]
    [HttpPatch("product/{productId:int}/me")]
    [Consumes("multipart/form-data")]
    public IActionResult UpdateMyReview(int productId, [FromForm] ProductReviewUpdateDto dto)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { message = "Danh gia chi duoc gui mot lan va khong the cap nhat lai." });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateReviewStatus(int id, ProductReviewStatusUpdateDto request)
    {
        var status = NormalizeReviewStatus(request.TrangThai) ?? NormalizeReviewStatus(request.Status);
        if (status is null)
        {
            return BadRequest(new { message = "Trang thai danh gia khong hop le." });
        }

        var review = await _dbContext.ProductReviews.FirstOrDefaultAsync(r => r.MaDanhGia == id);
        if (review is null)
        {
            return NotFound(new { message = "Khong tim thay danh gia." });
        }
        var oldValue = new { review.TrangThai, review.NgayCapNhat };

        review.TrangThai = status;
        review.NgayCapNhat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductReview", review.MaDanhGia.ToString(), "UpdateStatus", oldValue, new { review.TrangThai, review.NgayCapNhat });

        return Ok(new ProductReviewDto
        {
            MaDanhGia = review.MaDanhGia,
            MaSanPham = review.MaSanPham,
            MaNguoiDung = review.MaNguoiDung,
            TenNguoiDung = review.User?.HoTen,
            MaDonHang = review.MaDonHang,
            Diem = review.Diem,
            TieuDe = review.TieuDe,
            NoiDung = review.NoiDung,
            HinhAnhUrl = review.HinhAnhUrl,
            TrangThai = ToClientReviewStatus(review.TrangThai),
            NgayTao = review.NgayTao,
            NgayCapNhat = review.NgayCapNhat
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _dbContext.ProductReviews.FirstOrDefaultAsync(r => r.MaDanhGia == id);
        if (review is null)
        {
            return NotFound(new { message = "Khong tim thay danh gia." });
        }
        var oldValue = new { review.MaDanhGia, review.MaSanPham, review.MaNguoiDung, review.MaDonHang, review.Diem, review.TieuDe, review.TrangThai };

        _dbContext.ProductReviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        await _auditLog.WriteAsync(this, "ProductReview", id.ToString(), "Delete", oldValue, null);

        return NoContent();
    }

    private async Task<bool> ProductExistsAsync(int productId)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.MaSanPham == productId && p.DangHoatDong);
    }

    private async Task<int?> GetEligibleOrderIdAsync(int userId, int productId, int? requestedOrderId = null)
    {
        var query = _dbContext.ReviewOrders
            .AsNoTracking()
            .Where(o =>
                o.MaNguoiDung == userId &&
                o.Items.Any(i => i.MaSanPham == productId) &&
                (CompletedOrderStatuses.Contains(o.TrangThaiDonHang) ||
                 CompletedShippingStatuses.Contains(o.TrangThaiVanChuyen)));

        if (requestedOrderId.HasValue)
        {
            query = query.Where(o => o.MaDonHang == requestedOrderId.Value);
        }

        var orderId = await query
            .OrderByDescending(o => o.NgayTao)
            .Select(o => (int?)o.MaDonHang)
            .FirstOrDefaultAsync();

        return orderId;
    }

    private static ProductReviewDto ToReviewDto(ProductReview review)
    {
        return new ProductReviewDto
        {
            MaDanhGia = review.MaDanhGia,
            MaSanPham = review.MaSanPham,
            MaNguoiDung = review.MaNguoiDung,
            TenNguoiDung = review.User?.HoTen,
            MaDonHang = review.MaDonHang,
            Diem = review.Diem,
            TieuDe = review.TieuDe,
            NoiDung = review.NoiDung,
            HinhAnhUrl = review.HinhAnhUrl,
            TrangThai = ToClientReviewStatus(review.TrangThai),
            NgayTao = review.NgayTao,
            NgayCapNhat = review.NgayCapNhat
        };
    }

    private static string? ValidateReviewContent(byte rating, string? comment)
    {
        if (rating is < 1 or > 5)
        {
            return "Diem danh gia phai tu 1 den 5.";
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return "Noi dung binh luan khong duoc de trong.";
        }

        return null;
    }

    private static string? NormalizeReviewStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Equals(HiddenReviewStatus, StringComparison.OrdinalIgnoreCase))
        {
            return RejectedReviewStatus;
        }

        return AllowedReviewStatuses.FirstOrDefault(status => status.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string ToClientReviewStatus(string status)
    {
        return status.Equals(RejectedReviewStatus, StringComparison.OrdinalIgnoreCase) ? HiddenReviewStatus : status;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
