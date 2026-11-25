using System.Security.Claims;
using Drmc.Procurement.Domain.Entities.Organization;
using Drmc.Procurement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drmc.Procurement.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController(ApplicationDbContext db) : ControllerBase
{
    private int? CurrentUserId => int.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private bool IsSystemAdministrator => User.IsInRole("System Administrator");
    private bool IsUnitPPMPCoordinator => User.IsInRole("Unit PPMP Coordinator");

    [HttpGet("mine")]
    public async Task<ActionResult<List<TeamDto>>> GetMine()
    {
        if (!CurrentUserId.HasValue) return Forbid();

        var teams = await db.TeamMemberships
            .AsNoTracking()
            .Where(membership =>
                membership.UserId == CurrentUserId.Value &&
                membership.IsActive &&
                membership.Team.IsActive)
            .OrderBy(membership => membership.Team.TeamName)
            .Select(membership => new TeamDto(
                membership.Team.TeamId,
                membership.Team.DepartmentId,
                membership.Team.TeamCode,
                membership.Team.TeamName))
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(teams);
    }

    [HttpGet("department/{departmentId:int}")]
    [Authorize(Roles = "System Administrator,Unit PPMP Coordinator")]
    public async Task<ActionResult<List<TeamDto>>> GetByDepartment(int departmentId)
    {
        if (!IsSystemAdministrator && (!IsUnitPPMPCoordinator || !IsCurrentUserDepartment(departmentId))) return Forbid();

        var teams = await db.Teams
            .AsNoTracking()
            .Where(team => team.DepartmentId == departmentId && team.IsActive)
            .OrderBy(team => team.TeamName)
            .Select(team => new TeamDto(
                team.TeamId,
                team.DepartmentId,
                team.TeamCode,
                team.TeamName))
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(teams);
    }

    [HttpPost]
    [Authorize(Roles = "System Administrator,Unit PPMP Coordinator")]
    public async Task<ActionResult<TeamDto>> Create(CreateTeamRequest request)
    {
        if (!IsSystemAdministrator && (!IsUnitPPMPCoordinator || !IsCurrentUserDepartment(request.DepartmentId))) return Forbid();

        var departmentExists = await db.Departments.AnyAsync(
            department => department.DepartmentId == request.DepartmentId && department.IsActive,
            HttpContext.RequestAborted);
        if (!departmentExists) return BadRequest(new { message = "The department does not exist or is inactive." });

        var teamCode = request.TeamCode.Trim();
        var teamName = request.TeamName.Trim();
        if (string.IsNullOrWhiteSpace(teamCode) || string.IsNullOrWhiteSpace(teamName))
            return BadRequest(new { message = "Team code and team name are required." });

        var exists = await db.Teams.AnyAsync(team =>
            team.DepartmentId == request.DepartmentId && team.TeamCode == teamCode,
            HttpContext.RequestAborted);
        if (exists) return Conflict(new { message = "A team with this code already exists in the department." });

        var team = new Team
        {
            DepartmentId = request.DepartmentId,
            TeamCode = teamCode,
            TeamName = teamName
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync(HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetByDepartment), new { departmentId = team.DepartmentId },
            new TeamDto(team.TeamId, team.DepartmentId, team.TeamCode, team.TeamName));
    }

    [HttpPost("{teamId:int}/members/{userId:int}")]
    [Authorize(Roles = "System Administrator,Unit PPMP Coordinator")]
    public async Task<IActionResult> AddMember(int teamId, int userId)
    {
        var team = await db.Teams.FirstOrDefaultAsync(
            value => value.TeamId == teamId && value.IsActive,
            HttpContext.RequestAborted);
        var user = await db.Users.FirstOrDefaultAsync(
            value => value.UserId == userId && value.IsActive,
            HttpContext.RequestAborted);

        if (team == null || user == null) return NotFound(new { message = "The team or user was not found." });
        if (!IsSystemAdministrator && (!IsUnitPPMPCoordinator || !IsCurrentUserDepartment(team.DepartmentId))) return Forbid();
        if (user.DepartmentId != team.DepartmentId)
            return BadRequest(new { message = "A user can only join a team in their own department." });

        var membership = await db.TeamMemberships.FirstOrDefaultAsync(
            value => value.TeamId == teamId && value.UserId == userId,
            HttpContext.RequestAborted);
        if (membership == null)
        {
            db.TeamMemberships.Add(new TeamMembership { TeamId = teamId, UserId = userId });
        }
        else
        {
            membership.IsActive = true;
        }

        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpGet("department/{departmentId:int}/users")]
    [Authorize(Roles = "System Administrator,Unit PPMP Coordinator")]
    public async Task<ActionResult<List<TeamUserDto>>> GetDepartmentUsers(int departmentId)
    {
        if (!IsSystemAdministrator && (!IsUnitPPMPCoordinator || !IsCurrentUserDepartment(departmentId))) return Forbid();

        var users = await db.Users
            .AsNoTracking()
            .Where(user => user.DepartmentId == departmentId && user.IsActive)
            .OrderBy(user => user.FullName)
            .Select(user => new TeamUserDto(user.UserId, user.FullName, user.Email))
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(users);
    }

    private bool IsCurrentUserDepartment(int departmentId) =>
        int.TryParse(User.FindFirstValue("department_id"), out var currentDepartmentId) &&
        currentDepartmentId == departmentId;
}

public sealed record TeamDto(int TeamId, int DepartmentId, string TeamCode, string TeamName);
public sealed record TeamUserDto(int UserId, string FullName, string Email);

public sealed class CreateTeamRequest
{
    public int DepartmentId { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
}