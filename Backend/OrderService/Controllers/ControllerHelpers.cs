using System.Security.Claims;
using OrderService.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OrderService.Controllers;

internal static class ControllerHelpers
{
    public static int GetCurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue("sub");

        if (!int.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Token khong hop le.");
        }

        return userId;
    }

    public static bool CanManageOrders(this ControllerBase controller)
    {
        return controller.User.IsInRole("Admin") || controller.User.IsInRole("Staff");
    }

    public static IActionResult ToErrorResult(this ControllerBase controller, Exception ex)
    {
        switch (ex)
        {
            case BusinessException:
                return controller.BadRequest(new { message = ex.Message });
            case NotFoundException:
                return controller.NotFound(new { message = ex.Message });
            case ForbiddenException:
                return controller.Forbid();
            case UnauthorizedAccessException:
                return controller.Unauthorized(new { message = ex.Message });
            default:
                // Log unexpected exceptions with full detail so we can diagnose 500s instead of
                // returning an empty Problem body the frontend just shows as "status code 500".
                var logger = controller.HttpContext.RequestServices.GetService(typeof(ILogger<>).MakeGenericType(controller.GetType())) as ILogger;
                logger?.LogError(ex, "Unhandled exception in {Controller}", controller.GetType().Name);
                var inner = ex.InnerException;
                var detail = inner is null ? ex.Message : $"{ex.Message} | Inner: {inner.Message}";
                return new ObjectResult(new { message = ex.Message, detail, type = ex.GetType().Name })
                {
                    StatusCode = 500
                };
        }
    }
}
