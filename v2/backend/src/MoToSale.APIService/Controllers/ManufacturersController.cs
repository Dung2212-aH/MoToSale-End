using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoToSale.Common.Auth;
using MoToSale.DTO.Catalog;
using MoToSale.Services.Catalog;

namespace MoToSale.APIService.Controllers;

[ApiController]
[Route("api/manufacturers")]
public class ManufacturersController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public ManufacturersController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(new { items = await _catalog.GetManufacturersAsync() });

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPost]
    public async Task<IActionResult> Create(SaveManufacturerRequest request)
    {
        try { return Ok(new { id = await _catalog.CreateManufacturerAsync(request) }); }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = $"{RoleConstant.Admin},{RoleConstant.Staff}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveManufacturerRequest request)
    {
        try { await _catalog.UpdateManufacturerAsync(id, request); return Ok(new { id }); }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Roles = RoleConstant.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await _catalog.DeleteManufacturerAsync(id); return Ok(new { message = "Đã xóa." }); }
        catch (CatalogException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
