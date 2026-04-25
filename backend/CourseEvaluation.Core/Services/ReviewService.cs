using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;

namespace CourseEvaluation.Core.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICourseRepository _courseRepository;

    public ReviewService(IReviewRepository reviewRepository, ICourseRepository courseRepository)
    {
        _reviewRepository = reviewRepository;
        _courseRepository = courseRepository;
    }

    public async Task<ReviewDto> CreateOrUpdateReviewAsync(
        Guid userId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new ArgumentException("评分必须在1到5星之间");
        }

        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            throw new KeyNotFoundException("课程不存在");
        }

        var existingReview = await _reviewRepository.GetByUserAndCourseAsync(
            userId, request.CourseId, cancellationToken);

        if (existingReview != null)
        {
            existingReview.Rating = request.Rating;
            existingReview.Content = request.Content;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(existingReview, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            return MapToReviewDto(existingReview);
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = request.CourseId,
            Rating = request.Rating,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        return MapToReviewDto(review);
    }

    public async Task<ReviewDto?> GetUserReviewForCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId, cancellationToken);
        return review != null ? MapToReviewDto(review) : null;
    }

    public async Task<List<ReviewDto>> GetReviewsForCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await _reviewRepository.GetByCourseIdAsync(courseId, cancellationToken);
        return reviews.Select(MapToReviewDto).ToList();
    }

    public async Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

        if (review == null)
        {
            throw new KeyNotFoundException("评价不存在");
        }

        await _reviewRepository.DeleteAsync(review, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CanDeleteReviewAsync(
        Guid reviewId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

        if (review == null)
        {
            return false;
        }

        return isAdmin || review.UserId == userId;
    }

    public RatingDistribution GetRatingDistribution(List<Review> reviews)
    {
        var distribution = reviews
            .GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        return new RatingDistribution(
            Star1: distribution.TryGetValue(1, out var count1) ? count1 : 0,
            Star2: distribution.TryGetValue(2, out var count2) ? count2 : 0,
            Star3: distribution.TryGetValue(3, out var count3) ? count3 : 0,
            Star4: distribution.TryGetValue(4, out var count4) ? count4 : 0,
            Star5: distribution.TryGetValue(5, out var count5) ? count5 : 0);
    }

    private static ReviewDto MapToReviewDto(Review review)
    {
        return new ReviewDto(
            Id: review.Id,
            CourseId: review.CourseId,
            UserId: review.UserId,
            UserNickname: review.User?.Nickname ?? "未知用户",
            Rating: review.Rating,
            Content: review.Content,
            CreatedAt: review.CreatedAt,
            UpdatedAt: review.UpdatedAt);
    }
}
