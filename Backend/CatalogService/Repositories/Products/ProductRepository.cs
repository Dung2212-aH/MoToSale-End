using CatalogService.Data;
using CatalogService.DTOs.Products;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories.Products;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Product> QueryProducts(ProductSearchDto search)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            var keyword = search.Keyword.Trim();
            query = query.Where(p =>
                p.TenSanPham.Contains(keyword) ||
                p.MaSanPhamKinhDoanh.Contains(keyword) ||
                p.Slug.Contains(keyword));
        }

        if (search.MaDanhMuc.HasValue)
        {
            var maDanhMuc = search.MaDanhMuc.Value;
            query = query.Where(p =>
                p.MaDanhMuc == maDanhMuc ||
                _dbContext.Categories.Any(c => c.MaDanhMuc == p.MaDanhMuc && c.MaDanhMucCha == maDanhMuc));
        }

        if (search.MaHangXe.HasValue)
        {
            query = query.Where(p => p.MaHangXe == search.MaHangXe.Value);
        }

        if (search.MaDongXe.HasValue)
        {
            query = query.Where(p => p.MaDongXe == search.MaDongXe.Value);
        }

        if (search.MaDongXeTuongThich.HasValue)
        {
            var maDongXe = search.MaDongXeTuongThich.Value;
            query = query.Where(p =>
                _dbContext.PartCompatibilities.Any(pc =>
                    pc.DangHoatDong &&
                    pc.MaPhuTung == p.MaSanPham &&
                    (
                        pc.ApDungTatCaXe ||
                        pc.MaDongXe == maDongXe ||
                        (
                            pc.MaDongXe == null &&
                            pc.MaHangXe != null &&
                            _dbContext.VehicleModels.Any(vm => vm.MaDongXe == maDongXe && vm.MaHangXe == pc.MaHangXe)
                        )
                    )));
        }

        if (search.MaShowroom.HasValue)
        {
            query = query.Where(p => p.MaShowroom == search.MaShowroom.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.LoaiSanPham))
        {
            var loaiSanPham = search.LoaiSanPham.Trim();
            query = query.Where(p => p.LoaiSanPham == loaiSanPham);
        }

        if (!string.IsNullOrWhiteSpace(search.TrangThaiSanPham))
        {
            var trangThaiSanPham = search.TrangThaiSanPham.Trim();
            query = query.Where(p => p.TrangThaiSanPham == trangThaiSanPham);
        }

        if (search.GiaTu.HasValue)
        {
            query = query.Where(p => (p.GiaKhuyenMai ?? p.GiaGoc) >= search.GiaTu.Value);
        }

        if (search.GiaDen.HasValue)
        {
            query = query.Where(p => (p.GiaKhuyenMai ?? p.GiaGoc) <= search.GiaDen.Value);
        }

        if (search.DangHoatDong.HasValue)
        {
            query = query.Where(p => p.DangHoatDong == search.DangHoatDong.Value);
        }

        return ApplySort(query, search);
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, ProductSearchDto search)
    {
        var sortBy = search.SortBy?.Trim().ToLowerInvariant();

        return sortBy switch
        {
            "name" or "tensanpham" => search.SortDescending
                ? query.OrderByDescending(p => p.TenSanPham)
                : query.OrderBy(p => p.TenSanPham),
            "price" or "giaban" => search.SortDescending
                ? query.OrderByDescending(p => p.GiaKhuyenMai ?? p.GiaGoc)
                : query.OrderBy(p => p.GiaKhuyenMai ?? p.GiaGoc),
            "stock" or "soluongton" => search.SortDescending
                ? query.OrderByDescending(p => p.SoLuongTon)
                : query.OrderBy(p => p.SoLuongTon),
            "created" or "date" or "newest" => search.SortDescending
                ? query.OrderByDescending(p => p.NgayTao)
                : query.OrderBy(p => p.NgayTao),
            _ => search.SortDescending
                ? query.OrderByDescending(p => p.NgayTao)
                : query.OrderBy(p => p.NgayTao)
        };
    }

    public async Task<int> CountProductsAsync(ProductSearchDto search)
    {
        return await QueryProducts(search).CountAsync();
    }

    public async Task<List<Product>> GetProductsAsync(ProductSearchDto search)
    {
        var page = search.Page <= 0 ? 1 : search.Page;
        var pageSize = ProductSearchDto.NormalizePageSize(search.PageSize);

        return await QueryProducts(search)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int maSanPham)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.MaSanPham == maSanPham);
    }
}
