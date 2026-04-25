using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CourseEvaluation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateOrUpdateReview(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var review = await _reviewService.CreateOrUpdateReviewAsync(userId, request, cancellationToken);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");

        var canDelete = await _reviewService.CanDeleteReviewAsync(id, userId, isAdmin, cancellationToken);
        if (!canDelete)
        {
            return Forbid();
        }

        try
        {
            await _reviewService.DeleteReviewAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsForCourse(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsForCourseAsync(courseId, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("my/{courseId}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto?>> GetMyReviewForCourse(
        Guid courseId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var review = await _reviewService.GetUserReviewForCourseAsync(userId, courseId, cancellationToken);
        return review != null ? Ok(review) : NotFound();
    }
}
