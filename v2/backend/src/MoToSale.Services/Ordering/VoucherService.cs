using MoToSale.Common;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Ordering;
using MoToSale.Repository.EFCore;
using MoToSale.Repository.Ordering;

namespace MoToSale.Services.Ordering;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _vouchers;
    private readonly IRepository<UserVoucher> _userVouchers;

    public VoucherService(IVoucherRepository vouchers, IRepository<UserVoucher> userVouchers)
    {
        _vouchers = vouchers;
        _userVouchers = userVouchers;
    }

    public async Task<PagingResponse<VoucherDto>> SearchAsync(PagingRequest request)
    {
        var page = await _vouchers.SearchAsync(request);
        return new PagingResponse<VoucherDto>
        {
            Items = page.Items.Select(Map).ToList(),
            Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems,
        };
    }

    public async Task<VoucherDto?> GetAsync(int id)
    {
        var v = await _vouchers.GetByIdAsync(id);
        return v is null ? null : Map(v);
    }

    public async Task<int> CreateAsync(SaveVoucherRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) throw new VoucherException("Mã voucher là bắt buộc.");
        var code = r.Code.Trim().ToUpperInvariant();
        if (await _vouchers.CodeExistsAsync(code)) throw new VoucherException("Mã voucher đã tồn tại.");
        if (r.DiscountValue <= 0) throw new VoucherException("Giá trị giảm phải lớn hơn 0.");
        var v = new Voucher
        {
            Code = code, Description = r.Description, DiscountType = r.DiscountType == "Amount" ? "Amount" : "Percent",
            DiscountValue = r.DiscountValue, MaxDiscount = r.MaxDiscount, MinOrderValue = r.MinOrderValue,
            UsageLimit = r.UsageLimit, PerUserLimit = r.PerUserLimit, StartAt = r.StartAt, EndAt = r.EndAt,
            CreatedDate = DateTime.UtcNow, Status = (int)EntityStatus.Active,
        };
        _vouchers.Add(v);
        await _vouchers.SaveChangesAsync();
        return v.Id;
    }

    public async Task UpdateAsync(int id, SaveVoucherRequest r)
    {
        var v = await _vouchers.GetByIdAsync(id) ?? throw new VoucherException("Không tìm thấy voucher.");
        var code = r.Code.Trim().ToUpperInvariant();
        if (await _vouchers.CodeExistsAsync(code, id)) throw new VoucherException("Mã voucher đã tồn tại.");
        v.Code = code; v.Description = r.Description; v.DiscountType = r.DiscountType == "Amount" ? "Amount" : "Percent";
        v.DiscountValue = r.DiscountValue; v.MaxDiscount = r.MaxDiscount; v.MinOrderValue = r.MinOrderValue;
        v.UsageLimit = r.UsageLimit; v.PerUserLimit = r.PerUserLimit; v.StartAt = r.StartAt; v.EndAt = r.EndAt;
        v.Status = r.Status; v.UpdatedDate = DateTime.UtcNow;
        _vouchers.Update(v);
        await _vouchers.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var v = await _vouchers.GetByIdAsync(id) ?? throw new VoucherException("Không tìm thấy voucher.");
        _vouchers.Delete(v);
        await _vouchers.SaveChangesAsync();
    }

    public async Task<VoucherValidationResult> ValidateAsync(string code, decimal subtotal)
    {
        var v = await _vouchers.GetByCodeAsync(code.Trim().ToUpperInvariant());
        if (v is null) return new VoucherValidationResult(false, "Mã không tồn tại.", 0, null);
        if (v.Status != (int)EntityStatus.Active) return new VoucherValidationResult(false, "Voucher ngừng hoạt động.", 0, null);
        var now = DateTime.UtcNow;
        if (v.StartAt.HasValue && now < v.StartAt) return new VoucherValidationResult(false, "Voucher chưa bắt đầu.", 0, null);
        if (v.EndAt.HasValue && now > v.EndAt) return new VoucherValidationResult(false, "Voucher đã hết hạn.", 0, null);
        if (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit) return new VoucherValidationResult(false, "Voucher đã hết lượt.", 0, null);
        if (subtotal < v.MinOrderValue) return new VoucherValidationResult(false, $"Đơn tối thiểu {v.MinOrderValue:n0}đ.", 0, null);

        return new VoucherValidationResult(true, null, CalcDiscount(v, subtotal), Map(v));
    }

    public async Task<PagingResponse<VoucherDto>> GetPublicVouchersAsync(PagingRequest request)
    {
        var page = await _vouchers.GetActivePublicAsync(request);
        return new PagingResponse<VoucherDto>
        {
            Items = page.Items.Select(Map).ToList(),
            Page = page.Page, PageSize = page.PageSize, TotalItems = page.TotalItems,
        };
    }

    public async Task<IReadOnlyList<ApplicableVoucherDto>> GetApplicableAsync(int userId, decimal subtotal, string? orderType)
    {
        var now = DateTime.UtcNow;
        // Chỉ xét voucher người dùng đã lưu (Saved) và còn hiệu lực.
        var saved = await _userVouchers.FindAsync(uv => uv.UserId == userId && uv.VoucherStatus == UserVoucherStatus.Saved);
        if (saved.Count == 0) return Array.Empty<ApplicableVoucherDto>();

        var result = new List<ApplicableVoucherDto>();
        foreach (var uv in saved)
        {
            var v = await _vouchers.GetByIdAsync(uv.VoucherId);
            if (v is null) continue;
            if (v.Status != (int)EntityStatus.Active) continue;
            if (v.StartAt.HasValue && now < v.StartAt) continue;
            if (v.EndAt.HasValue && now > v.EndAt) continue;
            if (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit) continue;
            if (subtotal < v.MinOrderValue) continue;
            if (!string.IsNullOrWhiteSpace(v.OrderTypeRestriction) && v.OrderTypeRestriction != orderType) continue;

            result.Add(new ApplicableVoucherDto(Map(v), CalcDiscount(v, subtotal)));
        }

        return result
            .OrderBy(r => r.Voucher.MinOrderValue)
            .ThenByDescending(r => r.DiscountAmount)
            .ToList();
    }

    public async Task SaveForUserAsync(int userId, string code)
    {
        var v = await _vouchers.GetByCodeAsync(code.Trim().ToUpperInvariant())
            ?? throw new VoucherException("Voucher không tồn tại.");
        if (!v.IsPublic) throw new VoucherException("Voucher này không thể lưu.");
        if (v.Status != (int)EntityStatus.Active) throw new VoucherException("Voucher ngừng hoạt động.");
        var now = DateTime.UtcNow;
        if (v.StartAt.HasValue && now < v.StartAt) throw new VoucherException("Voucher chưa bắt đầu.");
        if (v.EndAt.HasValue && now > v.EndAt) throw new VoucherException("Voucher đã hết hạn.");
        if (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit) throw new VoucherException("Voucher đã hết lượt.");

        // Idempotent: nếu đã lưu (chưa dùng) thì không tạo bản ghi mới.
        var already = await _userVouchers.AnyAsync(uv =>
            uv.UserId == userId && uv.VoucherId == v.Id && uv.VoucherStatus == UserVoucherStatus.Saved);
        if (already) return;

        _userVouchers.Add(new UserVoucher
        {
            UserId = userId,
            VoucherId = v.Id,
            VoucherStatus = UserVoucherStatus.Saved,
            SavedAt = now,
            CreatedDate = now,
            Status = (int)EntityStatus.Active,
        });
        await _userVouchers.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<UserVoucherDto>> GetMyVouchersAsync(int userId)
    {
        var saved = await _userVouchers.FindAsync(uv => uv.UserId == userId && uv.VoucherStatus == UserVoucherStatus.Saved);
        if (saved.Count == 0) return Array.Empty<UserVoucherDto>();

        var list = new List<UserVoucherDto>();
        foreach (var uv in saved.OrderByDescending(x => x.SavedAt))
        {
            var v = await _vouchers.GetByIdAsync(uv.VoucherId);
            if (v is null) continue;
            list.Add(new UserVoucherDto(
                v.Id, v.Code, v.Description, v.DiscountType, v.DiscountValue, v.MaxDiscount, v.MinOrderValue,
                v.StartAt, v.EndAt, v.Scope, uv.VoucherStatus, uv.SavedAt, uv.UsedAt));
        }
        return list;
    }

    public async Task<int> CountMyVouchersAsync(int userId)
    {
        var saved = await _userVouchers.FindAsync(uv => uv.UserId == userId && uv.VoucherStatus == UserVoucherStatus.Saved);
        if (saved.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var count = 0;
        foreach (var uv in saved)
        {
            var v = await _vouchers.GetByIdAsync(uv.VoucherId);
            if (v is null) continue;
            if (v.Status != (int)EntityStatus.Active) continue;
            if (v.EndAt.HasValue && now > v.EndAt) continue;
            count++;
        }
        return count;
    }

    public async Task<VoucherDto?> GetByCodeAsync(string code)
    {
        var v = await _vouchers.GetByCodeAsync(code.Trim().ToUpperInvariant());
        return v is null ? null : Map(v);
    }

    /// <summary>Tính số tiền giảm — dùng chung cho Validate và Applicable.</summary>
    private static decimal CalcDiscount(Voucher v, decimal subtotal)
    {
        var discount = v.DiscountType == "Amount" ? v.DiscountValue : subtotal * v.DiscountValue / 100m;
        if (v.MaxDiscount.HasValue && discount > v.MaxDiscount) discount = v.MaxDiscount.Value;
        if (discount > subtotal) discount = subtotal;
        return decimal.Round(discount, 2);
    }

    private static VoucherDto Map(Voucher v) => new(
        v.Id, v.Code, v.Description, v.DiscountType, v.DiscountValue, v.MaxDiscount, v.MinOrderValue,
        v.UsageLimit, v.PerUserLimit, v.UsedCount, v.StartAt, v.EndAt, v.Status);
}
