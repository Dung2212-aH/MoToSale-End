using CatalogService.Data;
using CatalogService.DTOs.Contacts;
using CatalogService.DTOs.Faqs;
using CatalogService.DTOs.Posts;
using CatalogService.Entities;
using Microsoft.AspNetCore.Authorization;
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

        var contactRequest = new ContactRequest
        {
            HoTen = request.HoTen.Trim(),
            SoDienThoai = request.SoDienThoai.Trim(),
            Email = TrimToNull(request.Email)?.ToLowerInvariant(),
            TieuDe = TrimToNull(request.TieuDe),
            NoiDung = request.NoiDung.Trim(),
            LoaiYeuCau = contactType,
            MaSanPham = request.MaSanPham,
            TrangThai = "New",
            NgayTao = DateTime.UtcNow
        };

        _dbContext.ContactRequests.Add(contactRequest);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContactRequest), new { id = contactRequest.MaLienHe }, MapContactRequest(contactRequest));
    }

    [Authorize(Roles = "Admin,Staff")]
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
            TrangThai = contact.TrangThai,
            NgayTao = contact.NgayTao,
            DaXuLyLuc = contact.DaXuLyLuc
        };
    }

    // ===== Admin: Posts =====

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.Posts.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.TrangThai == status);

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                id = p.MaBaiViet,
                tieuDe = p.TieuDe,
                slug = p.Slug,
                tomTat = p.TomTat,
                anhDaiDienUrl = p.AnhDaiDienUrl,
                danhMuc = p.DanhMuc,
                trangThai = p.TrangThai,
                xuatBanLuc = p.XuatBanLuc,
                ngayTao = p.NgayTao
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems, totalPages = (int)Math.Ceiling(totalItems / (double)pageSize) });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("posts/{id:int}")]
    public async Task<IActionResult> GetPostById(int id)
    {
        var post = await _dbContext.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.MaBaiViet == id);
        if (post == null) return NotFound();
        return Ok(new
        {
            id = post.MaBaiViet,
            tieuDe = post.TieuDe,
            slug = post.Slug,
            tomTat = post.TomTat,
            noiDung = post.NoiDung,
            anhDaiDienUrl = post.AnhDaiDienUrl,
            danhMuc = post.DanhMuc,
            maTacGia = post.MaTacGia,
            trangThai = post.TrangThai,
            xuatBanLuc = post.XuatBanLuc,
            ngayTao = post.NgayTao,
            ngayCapNhat = post.NgayCapNhat
        });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var post = new Post
        {
            TieuDe = request.TieuDe.Trim(),
            Slug = request.Slug.Trim(),
            TomTat = TrimToNull(request.TomTat),
            NoiDung = request.NoiDung,
            AnhDaiDienUrl = TrimToNull(request.AnhDaiDienUrl),
            DanhMuc = TrimToNull(request.DanhMuc),
            MaTacGia = request.MaTacGia,
            TrangThai = request.TrangThai ?? "Draft",
            XuatBanLuc = request.XuatBanLuc,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPostById), new { id = post.MaBaiViet }, new { id = post.MaBaiViet });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("posts/{id:int}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.MaBaiViet == id);
        if (post == null) return NotFound();

        post.TieuDe = request.TieuDe.Trim();
        post.Slug = request.Slug.Trim();
        post.TomTat = TrimToNull(request.TomTat);
        post.NoiDung = request.NoiDung;
        post.AnhDaiDienUrl = TrimToNull(request.AnhDaiDienUrl);
        post.DanhMuc = TrimToNull(request.DanhMuc);
        post.TrangThai = request.TrangThai ?? post.TrangThai;
        post.XuatBanLuc = request.XuatBanLuc;
        post.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { id = post.MaBaiViet });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("posts/{id:int}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _dbContext.Posts.FirstOrDefaultAsync(p => p.MaBaiViet == id);
        if (post == null) return NotFound();

        _dbContext.Posts.Remove(post);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ===== Admin: FAQ =====

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("faq")]
    public async Task<IActionResult> GetFaqAdmin([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _dbContext.Faqs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(f => f.DanhMuc == category.Trim());

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            query = query.Where(f => f.CauHoi.ToLower().Contains(s) || f.CauTraLoi.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(f => f.ThuTuHienThi)
            .ThenBy(f => f.MaFAQ)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                id = f.MaFAQ,
                cauHoi = f.CauHoi,
                cauTraLoi = f.CauTraLoi,
                danhMuc = f.DanhMuc,
                thuTu = f.ThuTuHienThi,
                dangHoatDong = f.DangHoatDong
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPost("faq")]
    public async Task<IActionResult> CreateFaq([FromBody] CreateFaqRequest request)
    {
        var faq = new Faq
        {
            CauHoi = request.CauHoi.Trim(),
            CauTraLoi = request.CauTraLoi.Trim(),
            DanhMuc = TrimToNull(request.DanhMuc),
            ThuTuHienThi = request.ThuTuHienThi,
            DangHoatDong = request.DangHoatDong,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        _dbContext.Faqs.Add(faq);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFaqAdmin), null, new { id = faq.MaFAQ });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPut("faq/{id:int}")]
    public async Task<IActionResult> UpdateFaq(int id, [FromBody] UpdateFaqRequest request)
    {
        var faq = await _dbContext.Faqs.FirstOrDefaultAsync(f => f.MaFAQ == id);
        if (faq == null) return NotFound();

        faq.CauHoi = request.CauHoi.Trim();
        faq.CauTraLoi = request.CauTraLoi.Trim();
        faq.DanhMuc = TrimToNull(request.DanhMuc);
        faq.ThuTuHienThi = request.ThuTuHienThi;
        faq.DangHoatDong = request.DangHoatDong;
        faq.NgayCapNhat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { id = faq.MaFAQ });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpDelete("faq/{id:int}")]
    public async Task<IActionResult> DeleteFaq(int id)
    {
        var faq = await _dbContext.Faqs.FirstOrDefaultAsync(f => f.MaFAQ == id);
        if (faq == null) return NotFound();

        _dbContext.Faqs.Remove(faq);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    // ===== Admin: Contacts =====

    [Authorize(Roles = "Admin,Staff")]
    [HttpGet("contacts")]
    public async Task<IActionResult> GetContacts([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var query = _dbContext.ContactRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.TrangThai == status);

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                id = c.MaLienHe,
                hoTen = c.HoTen,
                soDienThoai = c.SoDienThoai,
                email = c.Email,
                tieuDe = c.TieuDe,
                noiDung = c.NoiDung,
                loaiYeuCau = c.LoaiYeuCau,
                trangThai = c.TrangThai,
                ngayTao = c.NgayTao,
                daXuLyLuc = c.DaXuLyLuc
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, totalItems, totalPages = (int)Math.Ceiling(totalItems / (double)pageSize) });
    }

    [Authorize(Roles = "Admin,Staff")]
    [HttpPatch("contacts/{id:int}/process")]
    public async Task<IActionResult> MarkContactProcessed(int id)
    {
        var contact = await _dbContext.ContactRequests.FirstOrDefaultAsync(c => c.MaLienHe == id);
        if (contact == null) return NotFound();

        contact.TrangThai = "Processed";
        contact.DaXuLyLuc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new { id = contact.MaLienHe, trangThai = contact.TrangThai, daXuLyLuc = contact.DaXuLyLuc });
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

// ===== Request DTOs for admin endpoints =====

public class CreatePostRequest
{
    public string TieuDe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? AnhDaiDienUrl { get; set; }
    public string? DanhMuc { get; set; }
    public int? MaTacGia { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? XuatBanLuc { get; set; }
}

public class UpdatePostRequest
{
    public string TieuDe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TomTat { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public string? AnhDaiDienUrl { get; set; }
    public string? DanhMuc { get; set; }
    public string? TrangThai { get; set; }
    public DateTime? XuatBanLuc { get; set; }
}

public class CreateFaqRequest
{
    public string CauHoi { get; set; } = string.Empty;
    public string CauTraLoi { get; set; } = string.Empty;
    public string? DanhMuc { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; } = true;
}

public class UpdateFaqRequest
{
    public string CauHoi { get; set; } = string.Empty;
    public string CauTraLoi { get; set; } = string.Empty;
    public string? DanhMuc { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool DangHoatDong { get; set; }
}
