using System.ComponentModel.DataAnnotations;

namespace ApiWeb.DTOs.Auth;

public record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password
);