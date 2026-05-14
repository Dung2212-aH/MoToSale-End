using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers;

internal static class ControllerHelpers
{
    public static int? GetCurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue("sub");

        return int.TryParse(value, out var userId) ? userId : null;
    }

}
