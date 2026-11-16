using Ar.Model;
using I.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SM.Model;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // Get "api/users"
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GetAll API started.");
        try
        {
            var users = await _userService.GetAllUsers();
            _logger.LogInformation("GetAll API completed successfully. Count: {Count}", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAll API failed.");
            return StatusCode(500, "Internal server error");
        }
    }

    // Get "api/users/{id}"
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("GetById API started.");
        try
        {
            var user = await _userService.GetUserById(id);
            _logger.LogInformation("User retrieved successfully. UserId: {UserId}", id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById API failed.");
            return StatusCode(500, "Internal server error");
        }
    }

    // Post "api/users"
    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        _logger.LogInformation("Create API started.");
        try
        {
            int id = await _userService.CreateUser(user);
            _logger.LogInformation("User created successfully. UserId: {UserId}", id);
            return Ok(new { Message = "User created", UserId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create API failed.");
            return StatusCode(500, "Internal server error");
        }
    }

    // Put "api/users/{id}"
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        _logger.LogInformation("Update API started.");
        try
        {
            var result = await _userService.UpdateUser(id, user);
            _logger.LogInformation("User updated successfully. UserId: {UserId}", id);
            if (result == false)
            {
                return NotFound("User not found");
            }
            return Ok("User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update API failed.");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/users/1/roles
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> AssignRoles(int id, AssignRole request)
    {
        _logger.LogInformation("AssignRoles API started.");
        try
        {
            await _userService.AssignRoles(id, request.RoleId);
            _logger.LogInformation("Roles assigned successfully. UserId: {UserId}", id);
            return Ok("Roles assigned successfully");
        }
        catch (SqlException ex) when (
            ex.Number == 50001 || ex.Number == 50002)
        {
            _logger.LogError(ex, "AssignRoles API failed.");
            return BadRequest(ex.Message);
        }
    }

    // GET: api/users/1/roles
    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(int id)
    {
        _logger.LogInformation("GetUserRoles API started.");
        try
        {
            var roles = await _userService.GetUserRoles(id);
            _logger.LogInformation("GetUserRoles completed. UserId: {UserId}", id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserRoles API failed.");
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/users/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("DeleteUser API started.");
        try
        {
            var result = await _userService.DeleteUser(id);
            _logger.LogInformation("DeleteUser completed. UserId: {UserId}", id);
            if (result)
            {
                return NotFound("User not found");
            }
            return Ok("User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUser API failed.");
            return StatusCode(500, "Internal server error");
        }
    }
}

