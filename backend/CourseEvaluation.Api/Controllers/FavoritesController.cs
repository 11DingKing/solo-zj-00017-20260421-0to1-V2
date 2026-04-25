using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseEvaluation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost("toggle/{courseId}")]
    public async Task<ActionResult<object>> ToggleFavorite(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isFavorited = await _favoriteService.ToggleFavoriteAsync(
                userId, courseId, cancellationToken);
            return Ok(new { isFavorited });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpGet("check/{courseId}")]
    public async Task<ActionResult<object>> CheckFavorite(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isFavorited = await _favoriteService.IsFavoriteAsync(
            userId, courseId, cancellationToken);
        return Ok(new { isFavorited });
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CourseDto>>> GetFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _favoriteService.GetFavoriteCoursesAsync(
            userId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
