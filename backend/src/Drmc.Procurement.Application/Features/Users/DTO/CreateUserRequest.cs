using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.Users.DTO;

public class CreateUserRequest
{
    public int? DepartmentId { get; set; }

    [Required]
    [StringLength(30)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Position { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}