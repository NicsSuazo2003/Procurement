using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.Auth.DTO;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
