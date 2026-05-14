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
    private static readonly HashSet<string> AllowedReviewStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Approved",
        "Rejected"
    };

    private readonly CatalogDbContext _dbContext;
    private readonly IImageStorageService _imageStorage;

    public ReviewsController(CatalogDbContext dbContext, IImageStorageService imageStorage)
    {
        _dbContext = dbContext;
        _imageStorage = imageStorage;
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.MaSanPham == productId && r.TrangThai == "Approved")
            .OrderByDescending(r => r.NgayTao)
            .Select(r => new ProductReviewDto
            {
                MaDanhGia = r.MaDanhGia,
                MaSanPham = r.MaSanPham,
                MaNguoiDung = r.MaNguoiDung,
                MaDonHang = r.MaDonHang,
                Diem = r.Diem,
                TieuDe = r.TieuDe,
                NoiDung = r.NoiDung,
                HinhAnhUrl = r.HinhAnhUrl,
                TrangThai = r.TrangThai,
                NgayTao = r.NgayTao
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("product/{productId:int}/summary")]
    public async Task<IActionResult> GetProductReviewSummary(int productId)
    {
        var summary = await _dbContext.ProductReviews
            .AsNoTracking()
            .Where(r => r.MaSanPham == productId && r.TrangThai == "Approved")
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
            .OrderByDescending(r => r.NgayTao)
            .Select(r => new ProductReviewDto
            {
                MaDanhGia = r.MaDanhGia,
                MaSanPham = r.MaSanPham,
                MaNguoiDung = r.MaNguoiDung,
                MaDonHang = r.MaDonHang,
                Diem = r.Diem,
                TieuDe = r.TieuDe,
                NoiDung = r.NoiDung,
                HinhAnhUrl = r.HinhAnhUrl,
                TrangThai = r.TrangThai,
                NgayTao = r.NgayTao
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromForm] ProductReviewCreateDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Token khong hop le." });
            }

            var productExists = await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => p.MaSanPham == dto.MaSanPham && p.DangHoatDong);

            if (!productExists)
            {
                return NotFound(new { message = "San pham khong ton tai hoac da ngung hoat dong." });
            }

            var review = new ProductReview
            {
                MaSanPham = dto.MaSanPham,
                MaNguoiDung = userId.Value,
                MaDonHang = dto.MaDonHang,
                Diem = dto.Diem,
                TieuDe = TrimToNull(dto.TieuDe),
                NoiDung = TrimToNull(dto.NoiDung),
                TrangThai = "Pending",
                NgayTao = DateTime.UtcNow
            };

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

            _dbContext.ProductReviews.Add(review);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Danh gia cua ban da duoc gui va dang cho duyet." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Da xay ra loi khi gui danh gia.", details = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateReviewStatus(int id, ProductReviewStatusUpdateDto request)
    {
        var status = NormalizeReviewStatus(request.TrangThai);
        if (status is null)
        {
            return BadRequest(new { message = "Trang thai danh gia khong hop le." });
        }

        var review = await _dbContext.ProductReviews.FirstOrDefaultAsync(r => r.MaDanhGia == id);
        if (review is null)
        {
            return NotFound(new { message = "Khong tim thay danh gia." });
        }

        review.TrangThai = status;
        await _dbContext.SaveChangesAsync();

        return Ok(new ProductReviewDto
        {
            MaDanhGia = review.MaDanhGia,
            MaSanPham = review.MaSanPham,
            MaNguoiDung = review.MaNguoiDung,
            MaDonHang = review.MaDonHang,
            Diem = review.Diem,
            TieuDe = review.TieuDe,
            NoiDung = review.NoiDung,
            HinhAnhUrl = review.HinhAnhUrl,
            TrangThai = review.TrangThai,
            NgayTao = review.NgayTao
        });
    }

    private static string? NormalizeReviewStatus(string value)
    {
        return AllowedReviewStatuses.FirstOrDefault(status => status.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
