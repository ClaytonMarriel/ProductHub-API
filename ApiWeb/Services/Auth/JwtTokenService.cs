using ApiWeb.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiWeb.Services.Auth;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public string CreateToken(ApplicationUser user)
    {
        var jwt = _config.GetSection("Jwt");
        var keyString = jwt["Key"];
        //var keyString = "a8f3d1c9b7e6k2m4p9r5t8w1x6z3q7v0n4y8u2i6o1p5l9s3b7e2m6k1r4t8x9";

        Console.WriteLine("JWT BYTES = " + Encoding.UTF8.GetByteCount(keyString ?? ""));
        Console.WriteLine($"JWT KEY: {keyString}");
        Console.WriteLine($"JWT LENGTH: {keyString?.Length}");


        if (string.IsNullOrWhiteSpace(keyString))
            throw new Exception("Jwt:Key não configurada.");

        if (Encoding.UTF8.GetByteCount(keyString) < 32)
            throw new Exception("Jwt:Key deve ter pelo menos 32 bytes.");

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.UserName ?? user.Email ?? "")
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}