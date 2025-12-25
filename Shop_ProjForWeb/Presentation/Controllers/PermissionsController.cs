using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Application.DTOs.Permission;
using Shop_ProjForWeb.Application.Interfaces;

namespace Shop_ProjForWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IPermissionService permissionService, ILogger<PermissionsController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        try
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPermissionById(int id)
    {
        try
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound(new { message = "Permission not found" });
            }
            return Ok(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission {PermissionId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
    {
        try
        {
            var permission = await _permissionService.CreatePermissionAsync(dto);
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto dto)
    {
        try
        {
            var permission = await _permissionService.UpdatePermissionAsync(id, dto);
            return Ok(permission);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Permission not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        try
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            if (result)
            {
                return Ok(new { message = "Permission deleted successfully" });
            }
            return NotFound(new { message = "Permission not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}

