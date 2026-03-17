using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs.Auth;

public record RegisterRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, MinLength(8)] string Password,
    string? FullName
);