using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Application.DTOs.User;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Infrastructure.Authorization;

namespace Shop_ProjForWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission("User", "Read")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("User", "Read")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    [RequirePermission("User", "Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("User", "Update")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("User", "Delete")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                return Ok(new { message = "User deleted successfully" });
            }
            return NotFound(new { message = "User not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost("{userId}/roles/{roleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(int userId, int roleId)
    {
        try
        {
            var result = await _userService.AssignRoleToUserAsync(userId, roleId);
            if (result)
            {
                return Ok(new { message = "Role assigned successfully" });
            }
            return BadRequest(new { message = "Role assignment failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{userId}/roles/{roleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        try
        {
            var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);
            if (result)
            {
                return Ok(new { message = "Role removed successfully" });
            }
            return BadRequest(new { message = "Role removal failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user");
            return BadRequest(new { message = ex.Message });
        }
    }
}

