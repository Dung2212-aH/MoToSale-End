using Microsoft.AspNetCore.Mvc;
using MoToSale.DTO.Auth;
using MoToSale.Services.Identity;

namespace MoToSale.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestCompat request)
    {
        try { return Ok(await _auth.RegisterAsync(request.ToRequest())); }
        catch (AuthException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestCompat request)
    {
        try { return Ok(await _auth.LoginAsync(request.ToRequest())); }
        catch (AuthException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var result = await _auth.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestCompat request)
    {
        try { await _auth.ResetPasswordAsync(request.ToRequest()); return Ok(new { message = "Đặt lại mật khẩu thành công." }); }
        catch (AuthException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
