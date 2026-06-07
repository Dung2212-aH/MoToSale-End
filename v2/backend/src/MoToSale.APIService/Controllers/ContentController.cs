using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.APIService.Models;
using MoToSale.APIService.Services;
using MoToSale.Common;
using MoToSale.Common.Auth;
using MoToSale.DTO.Common;
using MoToSale.DTO.Content;
using MoToSale.Repository.Ordering;
using MoToSale.Services.Content;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Route("api/content")]
public class ContentController : ControllerBase
{
    private const string StaffRoles = $"{RoleConstant.Admin},{RoleConstant.Staff}";

    private readonly IContentService _content;
    private readonly IImageStorage _storage;
    private readonly IVoucherRepository _vouchers;

    public ContentController(IContentService content, IImageStorage storage, IVoucherRepository vouchers)
    {
        _content = content;
        _storage = storage;
        _vouchers = vouchers;
    }

    private int? CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    // ===== Bài viết =====
    [Authorize(Roles = StaffRoles)]
    [HttpGet("posts")]
    public async Task<IActionResult> Posts([FromQuery] PagingRequest request, [FromQuery] string? status) => Ok(await _content.SearchPostsAsync(request, status));

    [Authorize(Roles = StaffRoles)]
    [HttpGet("posts/{id:int}")]
    public async Task<IActionResult> Post(int id) { var p = await _content.GetPostAsync(id); return p is null ? NotFound() : Ok(p); }

    [Authorize(Roles = StaffRoles)]
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost(SavePostRequest request)
    { try { return Ok(new { id = await _content.CreatePostAsync(request, CurrentUserId) }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpPut("posts/{id:int}")]
    public async Task<IActionResult> UpdatePost(int id, SavePostRequest request)
    { try { await _content.UpdatePostAsync(id, request); return Ok(new { id }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("posts/{id:int}")]
    public async Task<IActionResult> DeletePost(int id)
    { try { await _content.DeletePostAsync(id); return Ok(new { message = "Đã xóa." }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpPost("posts/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPostImage([FromForm] UploadFileRequest request)
    { try { return Ok(new { url = await _storage.SaveAsync(request.File, "posts", HttpContext.RequestAborted) }); } catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); } }

    // ===== Bài viết công khai (trang khách hàng) =====
    [AllowAnonymous]
    [HttpGet("blog-posts")]
    public async Task<IActionResult> BlogPosts([FromQuery] PagingRequest request) => Ok(await _content.GetPublishedPostsAsync(request));

    [AllowAnonymous]
    [HttpGet("blog-posts/{slug}")]
    public async Task<IActionResult> BlogPost(string slug)
    { var p = await _content.GetPublishedPostBySlugAsync(slug); return p is null ? NotFound() : Ok(p); }

    // ===== FAQ =====
    [HttpGet("faq")]
    public async Task<IActionResult> Faqs() => Ok(new { items = await _content.GetFaqsAsync() });

    // Alias công khai số nhiều cho SPA khách hàng
    [AllowAnonymous]
    [HttpGet("faqs")]
    public async Task<IActionResult> FaqsPlural() => Ok(new { items = await _content.GetFaqsAsync() });

    [Authorize(Roles = StaffRoles)]
    [HttpPost("faq")]
    public async Task<IActionResult> CreateFaq(SaveFaqRequest request)
    { try { return Ok(new { id = await _content.CreateFaqAsync(request) }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpPut("faq/{id:int}")]
    public async Task<IActionResult> UpdateFaq(int id, SaveFaqRequest request)
    { try { await _content.UpdateFaqAsync(id, request); return Ok(new { id }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("faq/{id:int}")]
    public async Task<IActionResult> DeleteFaq(int id)
    { try { await _content.DeleteFaqAsync(id); return Ok(new { message = "Đã xóa." }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    // ===== Liên hệ =====
    [AllowAnonymous]
    [HttpPost("contact-requests")]
    public async Task<IActionResult> SubmitContactRequest(ContactRequestForm request)
    { try { return Ok(new { id = await _content.CreateContactRequestAsync(request), message = "Đã gửi yêu cầu liên hệ." }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpGet("contacts")]
    public async Task<IActionResult> Contacts([FromQuery] PagingRequest request, [FromQuery] string? status) => Ok(await _content.SearchContactsAsync(request, status));

    [Authorize(Roles = StaffRoles)]
    [HttpPatch("contacts/{id:int}/process")]
    public async Task<IActionResult> ProcessContact(int id)
    { try { await _content.MarkContactProcessedAsync(id); return Ok(new { id }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    // ===== Banner trang chủ =====
    [HttpGet("home-banners")]
    public async Task<IActionResult> Banners([FromQuery] bool all = false) => Ok(new { items = await _content.GetBannersAsync(all) });

    [Authorize(Roles = StaffRoles)]
    [HttpPost("home-banners")]
    public async Task<IActionResult> CreateBanner(SaveBannerRequest request)
    { try { return Ok(new { id = await _content.CreateBannerAsync(request) }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpPut("home-banners/{id:int}")]
    public async Task<IActionResult> UpdateBanner(int id, SaveBannerRequest request)
    { try { await _content.UpdateBannerAsync(id, request); return Ok(new { id }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("home-banners/{id:int}")]
    public async Task<IActionResult> DeleteBanner(int id)
    { try { await _content.DeleteBannerAsync(id); return Ok(new { message = "Đã xóa." }); } catch (ContentException ex) { return BadRequest(new { message = ex.Message }); } }

    [Authorize(Roles = StaffRoles)]
    [HttpPost("home-banners/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBannerImage([FromForm] UploadFileRequest request)
    { try { return Ok(new { url = await _storage.SaveAsync(request.File, "banners", HttpContext.RequestAborted) }); } catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); } }

    // ===== Voucher công khai (trang voucher khách hàng) =====
    [AllowAnonymous]
    [HttpGet("vouchers/{code}")]
    public async Task<IActionResult> PublicVoucher(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return NotFound();
        var v = await _vouchers.GetByCodeAsync(code.Trim());
        // Chỉ trả về voucher tồn tại, đang hoạt động và được phép hiển thị công khai
        if (v is null || !v.IsPublic || v.Status != (int)EntityStatus.Active) return NotFound();
        return Ok(new PublicVoucherDto(v.Id, v.Code, v.Description, v.DiscountType, v.DiscountValue, v.MaxDiscount, v.MinOrderValue, v.StartAt, v.EndAt));
    }
}
