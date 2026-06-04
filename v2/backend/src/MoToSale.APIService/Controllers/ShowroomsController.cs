using Microsoft.AspNetCore.Mvc;
using MoToSale.Entities.SystemConfig;
using MoToSale.Repository.EFCore;

namespace MoToSale.APIService.Controllers;

/// <summary>Mô hình 1 cửa hàng: trả về thông tin cửa hàng duy nhất từ Cấu hình (Settings).</summary>
[ApiController]
[Route("api/showrooms")]
public class ShowroomsController : ControllerBase
{
    private readonly IRepository<Setting> _settings;

    public ShowroomsController(IRepository<Setting> settings) => _settings = settings;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var all = await _settings.GetAllAsync();
        var map = all.GroupBy(s => s.Key).ToDictionary(g => g.Key, g => g.First().Value);

        string? Pick(params string[] keys)
        {
            foreach (var k in keys)
                if (map.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        var store = new
        {
            id = 1,
            name = Pick("StoreName", "ShopName", "CompanyName") ?? "MoToSale",
            address = Pick("StoreAddress", "Address", "CompanyAddress") ?? "",
            phoneNumber = Pick("StorePhone", "Phone", "Hotline") ?? "",
            email = Pick("StoreEmail", "Email") ?? "",
            openingHours = Pick("OpeningHours", "StoreHours") ?? "08:00 - 21:00",
            // Thông tin chuyển khoản (khách quét QR thanh toán)
            bankName = Pick("BankName") ?? "",
            bankCode = Pick("BankCode", "BankBin") ?? "",          // mã ngân hàng VietQR (vd: VCB, TCB, BIDV)
            bankAccountNo = Pick("BankAccountNo", "BankAccount") ?? "",
            bankAccountName = Pick("BankAccountName", "StoreName") ?? "",
            bankQrUrl = Pick("BankQrUrl") ?? "",                   // ảnh QR cố định nếu shop tự tải lên
            isActive = true,
        };

        return Ok(new[] { store });
    }
}
