using Microsoft.AspNetCore.Mvc;
using MoToSale.APIService.Controllers;
using MoToSale.Backend.Tests.TestSupport;
using MoToSale.Repository.EFCore;

namespace MoToSale.Backend.Tests.Controllers;

public class OperationsControllerTests
{
    [Fact]
    public async Task SaveWarehouse_AcceptsEnglishRequestAndReturnsEnglishFields()
    {
        var f = new TestBackendFactory();
        await f.SeedCoreAsync();
        var controller = new OperationsController(
            new Repository<MoToSale.Entities.Catalog.Store>(f.Db),
            new Repository<MoToSale.Entities.SystemConfig.Setting>(f.Db));

        var result = await controller.SaveWarehouse(new WarehouseRequest
        {
            Name = "Warehouse English",
            Type = "Warehouse",
            AddressLine = "123 Test",
            Phone = "0900000001",
            IsActive = true,
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var listResult = await controller.GetWarehouses();
        var listOk = Assert.IsType<OkObjectResult>(listResult);
        var json = System.Text.Json.JsonSerializer.Serialize(listOk.Value);

        Assert.Contains("Warehouse English", json);
        Assert.Contains("addressLine", json);
        Assert.Contains("isActive", json);
    }

    [Fact]
    public async Task SaveSettings_AcceptsEnglishDescription()
    {
        var f = new TestBackendFactory();
        await f.SeedCoreAsync();
        var controller = new OperationsController(
            new Repository<MoToSale.Entities.Catalog.Store>(f.Db),
            new Repository<MoToSale.Entities.SystemConfig.Setting>(f.Db));

        var result = await controller.SaveSettings(new SettingsRequest
        {
            Items = [new SettingItem { Key = "store.openingHours", Value = "08:00-17:00", Description = "Business hours" }],
        });

        Assert.IsType<OkObjectResult>(result);
        var setting = Assert.Single(f.Db.Settings);
        Assert.Equal("Business hours", setting.Description);
    }
}
