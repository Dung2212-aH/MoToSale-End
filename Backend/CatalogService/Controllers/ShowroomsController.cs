using CatalogService.Data;
using CatalogService.DTOs.Showrooms;
using CatalogService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/showrooms")]
public class ShowroomsController : ControllerBase
{
    private readonly CatalogDbContext _dbContext;

    public ShowroomsController(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [HttpGet("~/api/stores")]
    public async Task<IActionResult> GetShowrooms([FromQuery] bool activeOnly = true)
    {
        var query = _dbContext.Showrooms.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(s => s.DangHoatDong);
        }

        var showrooms = await query.OrderBy(s => s.TenShowroom).Select(s => ToDto(s)).ToListAsync();
        return Ok(showrooms);
    }

    private static ShowroomDto ToDto(Showroom showroom)
    {
        return new ShowroomDto
        {
            MaShowroom = showroom.MaShowroom,
            TenShowroom = showroom.TenShowroom,
            Slug = showroom.Slug,
            DiaChi = showroom.DiaChi,
            SoDienThoai = showroom.SoDienThoai,
            Email = showroom.Email,
            GioMoCua = showroom.GioMoCua,
            DangHoatDong = showroom.DangHoatDong
        };
    }
}
