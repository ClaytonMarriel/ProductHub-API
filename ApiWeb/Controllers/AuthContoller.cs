using ApiWeb.DTOs.Auth;
using ApiWeb.Models.Auth;
using ApiWeb.Services.Auth;
using Microsoft.AspNetCore.Authentication;
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

    public AuthController(UserManager<ApplicationUser> userManager, JwtTokenService jwt)
    {
        _userManager = userManager;
        _jwt = jwt;
    }

    // 1) Cadastro
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

        var token = _jwt.CreateToken(user);
        return Ok(new { token });
    }

    // 2) Login (email+senha) => JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok) return Unauthorized();

        var token = _jwt.CreateToken(user);
        return Ok(new { token });
    }

    // 3) Google: redireciona pro login
    [HttpGet("google")]
    public IActionResult Google()
    {
        var props = new AuthenticationProperties
        {
            // ✅ depois que o middleware processar o retorno do Google,
            // ele vai redirecionar pra esse endpoint do controller:
            RedirectUri = "/api/auth/google-callback"
        };

        return Challenge(props, "Google");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        // lê a autenticação do cookie externo
        var result = await HttpContext.AuthenticateAsync("External");
        if (!result.Succeeded || result.Principal is null)
            return Unauthorized(new { error = "External authentication failed" });

        var email =
            result.Principal.FindFirstValue(ClaimTypes.Email) ??
            result.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "Google não retornou email." });

        // tenta achar usuário local
        var user = await _userManager.FindByEmailAsync(email);

        // se não existir, cria
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
        }

        // limpa cookie externo (boa prática)
        await HttpContext.SignOutAsync("External");

        // gera JWT da API
        var token = _jwt.CreateToken(user);
        return Ok(new { token });
    }

    [HttpGet("google-failed")]
    public IActionResult GoogleFailed([FromQuery] string? error)
     => BadRequest(new { error });
}