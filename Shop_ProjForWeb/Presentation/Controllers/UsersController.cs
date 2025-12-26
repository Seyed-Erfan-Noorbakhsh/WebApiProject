namespace Shop_ProjForWeb.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.DTOs;
using Shop_ProjForWeb.Core.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Exceptions;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService, IValidationService validationService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IValidationService _validationService = validationService;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetAllUsers([FromQuery] PaginatedRequest request)
    {
        var users = await _userService.GetAllUsersAsync();
        
        // Apply sorting
        var sortedUsers = request.SortBy.ToLower() switch
        {
            "fullname" => request.SortDescending ? users.OrderByDescending(u => u.FullName) : users.OrderBy(u => u.FullName),
            "isvip" => request.SortDescending ? users.OrderByDescending(u => u.IsVip) : users.OrderBy(u => u.IsVip),
            "createdat" => request.SortDescending ? users.OrderByDescending(u => u.CreatedAt) : users.OrderBy(u => u.CreatedAt),
            _ => request.SortDescending ? users.OrderByDescending(u => u.Id) : users.OrderBy(u => u.Id)
        };

        // Apply pagination
        var totalCount = sortedUsers.Count();
        var pagedUsers = sortedUsers
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = new PaginatedResponse<UserDto>
        {
            Items = pagedUsers,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        // Use ValidationService for consistent validation
        var validationResult = await _validationService.ValidateBusinessRulesAsync(dto, "create");
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
            return BadRequest(new { error = "Validation failed", validationErrors = errors });
        }

        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        // Validate if FullName is provided
        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var nameValidation = _validationService.ValidateRequired(dto.FullName, "FullName");
            if (!nameValidation.IsValid)
            {
                var errors = nameValidation.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return BadRequest(new { error = "Validation failed", validationErrors = errors });
            }

            if (dto.FullName.Length < 2 || dto.FullName.Length > 100)
            {
                return BadRequest(new { error = "Full name must be between 2 and 100 characters" });
            }
        }

        await _userService.UpdateUserAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
