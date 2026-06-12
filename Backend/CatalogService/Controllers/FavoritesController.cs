using CatalogService.Data;
using CatalogService.Entities;
using CatalogService.Repositories.ProductImages;
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
    private readonly IProductImageRepository _productImageRepository;

    public FavoritesController(CatalogDbContext dbContext, IProductImageRepository productImageRepository)
    {
        _dbContext = dbContext;
        _productImageRepository = productImageRepository;
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

        var productIds = rows.Select(row => row.product.MaSanPham).ToList();
        var imageMap = await _productImageRepository.GetPrimaryImageUrlsAsync(productIds);
        var priceMap = await GetVariantPriceMapAsync(productIds);

        return Ok(rows.Select(row => ToFavorite(row.favorite, row.product, imageMap, priceMap)));
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

        var imageMap = await _productImageRepository.GetPrimaryImageUrlsAsync(new[] { product.MaSanPham });
        var priceMap = await GetVariantPriceMapAsync(new[] { product.MaSanPham });

        return Ok(ToFavorite(favorite, product, imageMap, priceMap));
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

    private static object ToFavorite(Favorite favorite, Product product, IReadOnlyDictionary<int, string> imageMap, IReadOnlyDictionary<int, FavoritePrice> priceMap)
    {
        return new
        {
            maNguoiDung = favorite.MaNguoiDung,
            maSanPham = favorite.MaSanPham,
            ngayTao = favorite.NgayTao,
            product = ToProduct(product, imageMap, priceMap)
        };
    }

    private static object ToProduct(Product product, IReadOnlyDictionary<int, string> imageMap, IReadOnlyDictionary<int, FavoritePrice> priceMap)
    {
        imageMap.TryGetValue(product.MaSanPham, out var anhChinhUrl);
        priceMap.TryGetValue(product.MaSanPham, out var price);
        var giaKhuyenMai = price.GiaBan > 0 && price.GiaBan < price.GiaGoc ? price.GiaBan : (decimal?)null;

        return new
        {
            maSanPham = product.MaSanPham,
            maSanPhamKinhDoanh = product.MaSanPhamKinhDoanh,
            tenSanPham = product.TenSanPham,
            slug = product.Slug,
            anhChinhUrl = anhChinhUrl ?? product.AnhChinhUrl,
            maDanhMuc = product.MaDanhMuc,
            maHangXe = product.MaHangXe,
            maDongXe = product.MaDongXe,
            loaiSanPham = product.LoaiSanPham,
            // Giá tổng hợp từ biến thể (giá thật nằm ở BIENSANPHAM).
            giaThapNhat = price.GiaBan,
            giaGocThapNhat = price.GiaGoc,
            giaKhuyenMai,
            giaBan = price.GiaBan,
            tyLeGiam = GetDiscountPercent(price.GiaGoc, price.GiaBan),
            soLuongTon = price.TongTon,
            dangHoatDong = product.DangHoatDong,
            trangThaiSanPham = product.TrangThaiSanPham
        };
    }

    private static decimal? GetDiscountPercent(decimal giaGoc, decimal giaBan)
    {
        if (giaGoc <= 0 || giaBan <= 0 || giaBan >= giaGoc)
        {
            return null;
        }

        return Math.Round((giaGoc - giaBan) * 100m / giaGoc, 1, MidpointRounding.AwayFromZero);
    }

    private async Task<Dictionary<int, FavoritePrice>> GetVariantPriceMapAsync(IReadOnlyCollection<int> productIds)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<int, FavoritePrice>();
        }

        var rows = await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(v => productIds.Contains(v.MaSanPham))
            .GroupBy(v => v.MaSanPham)
            .Select(g => new
            {
                MaSanPham = g.Key,
                GiaGocMin = g.Min(x => (decimal?)x.GiaGoc) ?? 0m,
                GiaBanMin = g.Min(x => (decimal?)(x.GiaKhuyenMai ?? x.GiaGoc)) ?? 0m,
                TongTon = g.Sum(x => x.SoLuongTon ?? 0)
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.MaSanPham, r => new FavoritePrice(r.GiaGocMin, r.GiaBanMin, r.TongTon));
    }

    private readonly record struct FavoritePrice(decimal GiaGoc, decimal GiaBan, int TongTon);
}
