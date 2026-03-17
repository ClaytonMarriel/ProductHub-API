using ApiWeb.Constants;
using ApiWeb.DTOs.Auth;
using ApiWeb.Models.Auth;
using ApiWeb.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiWeb.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwt;
    private readonly RefreshTokenService _refreshTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwt,
        RefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _jwt = jwt;
        _refreshTokenService = refreshTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, Roles.User);

        var accessToken = await _jwt.CreateTokenAsync(user);
        var refreshToken = await _refreshTokenService.CreateAsync(user);

        return Ok(new AuthResponse(accessToken, refreshToken));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Email ou senha inválidos." });

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            return Unauthorized(new { message = "Email ou senha inválidos." });

        var accessToken = await _jwt.CreateTokenAsync(user);
        var refreshToken = await _refreshTokenService.CreateAsync(user);

        return Ok(new AuthResponse(accessToken, refreshToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Unauthorized();

        return Ok(new UserMeResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName
        ));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenService.GetValidTokenAsync(request.RefreshToken);

        if (storedToken is null)
            return Unauthorized(new { message = "Refresh token inválido ou expirado." });

        await _refreshTokenService.RevokeAsync(request.RefreshToken);

        var newAccessToken = await _jwt.CreateTokenAsync(storedToken.User);
        var newRefreshToken = await _refreshTokenService.CreateAsync(storedToken.User);

        return Ok(new AuthResponse(newAccessToken, newRefreshToken));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpGet("google")]
    public IActionResult Google()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/google-callback"
        };

        return Challenge(props, "Google");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
            return Unauthorized(new { error = "External authentication failed" });

        var email =
            result.Principal.FindFirstValue(ClaimTypes.Email) ??
            result.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "Google não retornou email." });

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            var fullName =
                result.Principal.FindFirstValue(ClaimTypes.Name) ??
                $"{result.Principal.FindFirstValue(ClaimTypes.GivenName)} {result.Principal.FindFirstValue(ClaimTypes.Surname)}".Trim();

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName
            };

            var create = await _userManager.CreateAsync(user);
            if (!create.Succeeded)
                return BadRequest(new { errors = create.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, Roles.User);
        }

        await HttpContext.SignOutAsync("External");

        var accessToken = await _jwt.CreateTokenAsync(user);
        var refreshToken = await _refreshTokenService.CreateAsync(user);

        return Ok(new AuthResponse(accessToken, refreshToken));
    }

    [HttpGet("google-failed")]
    public IActionResult GoogleFailed([FromQuery] string? error)
        => BadRequest(new { error });
}