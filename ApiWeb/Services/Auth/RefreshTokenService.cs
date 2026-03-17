using ApiWeb.Data;
using ApiWeb.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ApiWeb.Services.Auth;

public class RefreshTokenService
{
    private readonly AppDbContent _context;

    public RefreshTokenService(AppDbContent context)
    {
        _context = context;
    }

    public async Task<string> CreateAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(ct);

        return token;
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.Token == token &&
                !r.IsRevoked &&
                r.ExpiresAt > DateTime.UtcNow,
                ct);
    }

    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, ct);

        if (refreshToken is null)
            return;

        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync(ct);
    }
}