namespace ApiWeb.DTOs.Auth
{
    public sealed record UserMeResponse(
        string Id,
        string Email,
        string? Fullname)
    {
       
    }
}
