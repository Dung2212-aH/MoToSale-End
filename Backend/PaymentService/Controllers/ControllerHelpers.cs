using System.Security.Claims;
using PaymentService.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Controllers;

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

    public static bool CanManagePayments(this ControllerBase controller)
    {
        return controller.User.IsInRole("Admin") || controller.User.IsInRole("Staff");
    }

    public static IActionResult ToErrorResult(this ControllerBase controller, Exception ex)
    {
        return ex switch
        {
            BusinessException => controller.BadRequest(new { message = ex.Message }),
            NotFoundException => controller.NotFound(new { message = ex.Message }),
            ForbiddenException => controller.Forbid(),
            UnauthorizedAccessException => controller.Unauthorized(new { message = ex.Message }),
            _ => controller.Problem(ex.Message)
        };
    }
}
