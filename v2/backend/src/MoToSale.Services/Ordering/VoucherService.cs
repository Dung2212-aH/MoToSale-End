using MoToSale.Common;
using MoToSale.DTO.Common;
using MoToSale.DTO.Ordering;
using MoToSale.Entities.Ordering;
using MoToSale.Repository.Ordering;

namespace MoToSale.Services.Ordering;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _vouchers;

    public VoucherService(IVoucherRepository vouchers) => _vouchers = vouchers;

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
        if (v.UsedCount > 0)
            throw new VoucherException("Voucher đã được sử dụng trong đơn hàng, không thể xóa. Hãy đặt trạng thái Ngừng thay vì xóa.");
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

        var discount = v.DiscountType == "Amount" ? v.DiscountValue : subtotal * v.DiscountValue / 100m;
        if (v.MaxDiscount.HasValue && discount > v.MaxDiscount) discount = v.MaxDiscount.Value;
        if (discount > subtotal) discount = subtotal;
        return new VoucherValidationResult(true, null, decimal.Round(discount, 2), Map(v));
    }

    private static VoucherDto Map(Voucher v) => new(
        v.Id, v.Code, v.Description, v.DiscountType, v.DiscountValue, v.MaxDiscount, v.MinOrderValue,
        v.UsageLimit, v.PerUserLimit, v.UsedCount, v.StartAt, v.EndAt, v.Status);
}
