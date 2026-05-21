using CatalogService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly CatalogDbContext _db;
    public InventoryController(CatalogDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;
        var items = await _db.Database.SqlQueryRaw<InventoryRow>(
            "SELECT MaSanPham, MaBienSanPham, TenSanPham, TenBienThe, TonKhoThucTe, SoLuongDangGiu, TonKhoKhaDung FROM v_TONKHO_KHADUNG ORDER BY TenSanPham OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY",
            offset, pageSize).ToListAsync();

        var total = await _db.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM v_TONKHO_KHADUNG").FirstOrDefaultAsync();

        return Ok(new { items, page, pageSize, totalItems = total, totalPages = (int)Math.Ceiling(total / (double)pageSize) });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        await _db.Database.ExecuteSqlRawAsync("EXEC sp_SANPHAM_DongBoTatCaSoLuongTon");
        return Ok(new { message = "Đồng bộ tồn kho thành công." });
    }
}

public class InventoryRow
{
    public int MaSanPham { get; set; }
    public int? MaBienSanPham { get; set; }
    public string TenSanPham { get; set; } = "";
    public string? TenBienThe { get; set; }
    public int TonKhoThucTe { get; set; }
    public int SoLuongDangGiu { get; set; }
    public int TonKhoKhaDung { get; set; }
}
