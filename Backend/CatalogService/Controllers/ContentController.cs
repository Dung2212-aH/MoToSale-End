using CatalogService.Data;
using CatalogService.DTOs.Contacts;
using CatalogService.DTOs.Faqs;
using CatalogService.DTOs.Posts;
using CatalogService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/content")]
public class ContentController : ControllerBase
{
    private static readonly HashSet<string> AllowedContactTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "General",
        "Product",
        "TestDrive",
        "Consultation"
    };

    private readonly CatalogDbContext _dbContext;

    public ContentController(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("blog-posts")]
    public async Task<IActionResult> GetBlogPosts(
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var now = DateTime.UtcNow;
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.Posts
            .AsNoTracking()
            .Where(p => p.TrangThai == "Published" && (!p.XuatBanLuc.HasValue || p.XuatBanLuc <= now));

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            query = query.Where(p => p.DanhMuc == normalizedCategory);
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.XuatBanLuc ?? p.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListItemDto
            {
                MaBaiViet = p.MaBaiViet,
                TieuDe = p.TieuDe,
                Slug = p.Slug,
                TomTat = p.TomTat,
                AnhDaiDienUrl = p.AnhDaiDienUrl,
                DanhMuc = p.DanhMuc,
                XuatBanLuc = p.XuatBanLuc,
                TrangThai = p.TrangThai
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems });
    }

    [HttpGet("blog-posts/{slug}")]
    public async Task<IActionResult> GetBlogPostBySlug(string slug)
    {
        var now = DateTime.UtcNow;
        var post = await _dbContext.Posts
            .AsNoTracking()
            .Where(p => p.Slug == slug && p.TrangThai == "Published" && (!p.XuatBanLuc.HasValue || p.XuatBanLuc <= now))
            .Select(p => new PostDetailDto
            {
                MaBaiViet = p.MaBaiViet,
                TieuDe = p.TieuDe,
                Slug = p.Slug,
                TomTat = p.TomTat,
                NoiDung = p.NoiDung,
                AnhDaiDienUrl = p.AnhDaiDienUrl,
                DanhMuc = p.DanhMuc,
                MaTacGia = p.MaTacGia,
                XuatBanLuc = p.XuatBanLuc,
                TrangThai = p.TrangThai
            })
            .FirstOrDefaultAsync();

        return post is null ? NotFound() : Ok(post);
    }

    [HttpGet("faqs")]
    public async Task<IActionResult> GetFaqs([FromQuery] string? category = null)
    {
        var query = _dbContext.Faqs
            .AsNoTracking()
            .Where(f => f.DangHoatDong);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            query = query.Where(f => f.DanhMuc == normalizedCategory);
        }

        var items = await query
            .OrderBy(f => f.ThuTuHienThi)
            .ThenBy(f => f.MaFAQ)
            .Select(f => new FaqDto
            {
                MaFAQ = f.MaFAQ,
                CauHoi = f.CauHoi,
                CauTraLoi = f.CauTraLoi,
                DanhMuc = f.DanhMuc,
                ThuTuHienThi = f.ThuTuHienThi,
                DangHoatDong = f.DangHoatDong
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("contact-requests")]
    public async Task<IActionResult> CreateContactRequest(ContactRequestCreateDto request)
    {
        var contactType = NormalizeContactType(request.LoaiYeuCau);
        if (contactType is null)
        {
            return BadRequest(new { message = "Loai yeu cau khong hop le." });
        }

        if (request.MaSanPham.HasValue &&
            !await _dbContext.Products.AsNoTracking().AnyAsync(p => p.MaSanPham == request.MaSanPham.Value && p.DangHoatDong))
        {
            return BadRequest(new { message = "San pham khong ton tai hoac da ngung hoat dong." });
        }

        if (request.MaShowroom.HasValue &&
            !await _dbContext.Showrooms.AsNoTracking().AnyAsync(s => s.MaShowroom == request.MaShowroom.Value && s.DangHoatDong))
        {
            return BadRequest(new { message = "Showroom khong ton tai hoac da ngung hoat dong." });
        }

        var contactRequest = new ContactRequest
        {
            HoTen = request.HoTen.Trim(),
            SoDienThoai = request.SoDienThoai.Trim(),
            Email = TrimToNull(request.Email)?.ToLowerInvariant(),
            TieuDe = TrimToNull(request.TieuDe),
            NoiDung = request.NoiDung.Trim(),
            LoaiYeuCau = contactType,
            MaSanPham = request.MaSanPham,
            MaShowroom = request.MaShowroom,
            TrangThai = "New",
            NgayTao = DateTime.UtcNow
        };

        _dbContext.ContactRequests.Add(contactRequest);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContactRequest), new { id = contactRequest.MaLienHe }, MapContactRequest(contactRequest));
    }

    [HttpGet("contact-requests/{id:int}")]
    public async Task<IActionResult> GetContactRequest(int id)
    {
        var contact = await _dbContext.ContactRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MaLienHe == id);

        return contact is null ? NotFound() : Ok(MapContactRequest(contact));
    }

    private static string? NormalizeContactType(string value)
    {
        return AllowedContactTypes.FirstOrDefault(type => type.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static ContactRequestDto MapContactRequest(ContactRequest contact)
    {
        return new ContactRequestDto
        {
            MaLienHe = contact.MaLienHe,
            HoTen = contact.HoTen,
            SoDienThoai = contact.SoDienThoai,
            Email = contact.Email,
            TieuDe = contact.TieuDe,
            NoiDung = contact.NoiDung,
            LoaiYeuCau = contact.LoaiYeuCau,
            MaSanPham = contact.MaSanPham,
            MaShowroom = contact.MaShowroom,
            TrangThai = contact.TrangThai,
            NgayTao = contact.NgayTao,
            DaXuLyLuc = contact.DaXuLyLuc
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
