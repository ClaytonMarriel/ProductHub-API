namespace ApiWeb.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken
);