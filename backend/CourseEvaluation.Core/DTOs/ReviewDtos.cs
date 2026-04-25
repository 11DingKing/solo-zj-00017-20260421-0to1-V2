namespace CourseEvaluation.Core.DTOs;

public record ReviewDto(
    Guid Id,
    Guid CourseId,
    Guid UserId,
    string UserNickname,
    int Rating,
    string? Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateReviewRequest(
    Guid CourseId,
    int Rating,
    string? Content);

public record UpdateReviewRequest(
    int Rating,
    string? Content);

public record RatingDistribution(
    int Star1,
    int Star2,
    int Star3,
    int Star4,
    int Star5);

public record CourseDetailDto(
    CourseDto Course,
    List<ReviewDto> Reviews,
    RatingDistribution RatingDistribution,
    bool HasUserReviewed,
    ReviewDto? UserReview);
