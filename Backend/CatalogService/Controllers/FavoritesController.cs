using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[Authorize]
[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly CatalogDbContext _dbContext;

    public FavoritesController(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = this.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var rows = await _dbContext.Favorites
            .AsNoTracking()
            .Where(f => f.MaNguoiDung == userId.Value)
            .Join(
                _dbContext.Products.AsNoTracking().Where(p => p.DangHoatDong),
                favorite => favorite.MaSanPham,
                product => product.MaSanPham,
                (favorite, product) => new { favorite, product })
            .OrderByDescending(row => row.favorite.NgayTao)
            .ToListAsync();

        return Ok(rows.Select(row => ToFavorite(row.favorite, row.product)));
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId)
    {
        var userId = this.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.MaSanPham == productId && p.DangHoatDong);

        if (product is null)
        {
            return NotFound(new { message = "San pham khong ton tai hoac da ngung hoat dong." });
        }

        var favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.MaNguoiDung == userId.Value && f.MaSanPham == productId);

        if (favorite is null)
        {
            favorite = new Favorite
            {
                MaNguoiDung = userId.Value,
                MaSanPham = productId,
                NgayTao = DateTime.UtcNow
            };

            _dbContext.Favorites.Add(favorite);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(ToFavorite(favorite, product));
    }

    [HttpPost]
    public async Task<IActionResult> AddFromBody(FavoriteRequest request)
    {
        return await Add(request.MaSanPham);
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = this.GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var favorite = await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.MaNguoiDung == userId.Value && f.MaSanPham == productId);

        if (favorite is not null)
        {
            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    private static object ToFavorite(Favorite favorite, Product product)
    {
        return new
        {
            maNguoiDung = favorite.MaNguoiDung,
            maSanPham = favorite.MaSanPham,
            ngayTao = favorite.NgayTao,
            product = ToProduct(product)
        };
    }

    private static object ToProduct(Product product)
    {
        return new
        {
            maSanPham = product.MaSanPham,
            maSanPhamKinhDoanh = product.MaSanPhamKinhDoanh,
            tenSanPham = product.TenSanPham,
            slug = product.Slug,
            maDanhMuc = product.MaDanhMuc,
            maHangXe = product.MaHangXe,
            maDongXe = product.MaDongXe,
            loaiSanPham = product.LoaiSanPham,
            giaGoc = product.GiaGoc,
            giaKhuyenMai = product.GiaKhuyenMai,
            giaBan = product.GiaKhuyenMai ?? product.GiaGoc,
            tyLeGiam = GetDiscountPercent(product),
            soLuongTon = product.SoLuongTon,
            dangHoatDong = product.DangHoatDong,
            trangThaiSanPham = product.TrangThaiSanPham
        };
    }

    private static int? GetDiscountPercent(Product product)
    {
        if (!product.GiaKhuyenMai.HasValue || product.GiaGoc <= 0 || product.GiaKhuyenMai.Value >= product.GiaGoc)
        {
            return null;
        }

        return (int)Math.Round((product.GiaGoc - product.GiaKhuyenMai.Value) * 100 / product.GiaGoc);
    }
}

public class FavoriteRequest
{
    public int MaSanPham { get; set; }
}
