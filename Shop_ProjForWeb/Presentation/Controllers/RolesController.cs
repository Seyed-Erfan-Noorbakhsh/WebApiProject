using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Application.DTOs.Role;
using Shop_ProjForWeb.Application.Interfaces;

namespace Shop_ProjForWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(int id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleService.UpdateRoleAsync(id, dto);
            return Ok(role);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Role not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var result = await _roleService.DeleteRoleAsync(id);
            if (result)
            {
                return Ok(new { message = "Role deleted successfully" });
            }
            return NotFound(new { message = "Role not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> AssignPermission(int roleId, int permissionId)
    {
        try
        {
            var result = await _roleService.AssignPermissionToRoleAsync(roleId, permissionId);
            if (result)
            {
                return Ok(new { message = "Permission assigned successfully" });
            }
            return BadRequest(new { message = "Permission assignment failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission to role");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermission(int roleId, int permissionId)
    {
        try
        {
            var result = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
            if (result)
            {
                return Ok(new { message = "Permission removed successfully" });
            }
            return BadRequest(new { message = "Permission removal failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission from role");
            return BadRequest(new { message = ex.Message });
        }
    }
}

