using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseEvaluation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _authService.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Register), user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Nickname: user.Nickname,
            Role: user.Role.ToString(),
            CreatedAt: user.CreatedAt));
    }
}
