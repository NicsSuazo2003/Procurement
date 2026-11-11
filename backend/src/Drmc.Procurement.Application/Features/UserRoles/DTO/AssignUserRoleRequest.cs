using System.ComponentModel.DataAnnotations;

namespace Drmc.Procurement.Application.Features.UserRoles.DTO;

public class AssignUserRoleRequest
{
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }
}