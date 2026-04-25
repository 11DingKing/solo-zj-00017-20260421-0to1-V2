using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;

namespace CourseEvaluation.Core.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateOrUpdateReviewAsync(
        Guid userId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ReviewDto?> GetUserReviewForCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<List<ReviewDto>> GetReviewsForCourseAsync(
        Guid courseId,
        ReviewSortBy? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

    Task<bool> CanDeleteReviewAsync(
        Guid reviewId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    RatingDistribution GetRatingDistribution(List<Review> reviews);
}
