using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;

namespace CourseEvaluation.Core.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IReviewService _reviewService;

    public CourseService(
        ICourseRepository courseRepository,
        IReviewRepository reviewRepository,
        IReviewService reviewService)
    {
        _courseRepository = courseRepository;
        _reviewRepository = reviewRepository;
        _reviewService = reviewService;
    }

    public async Task<PagedResult<CourseDto>> GetCoursesAsync(
        CourseListRequest request,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _courseRepository.SearchAndSortAsync(request, cancellationToken);

        var courseDtos = pagedResult.Items.Select(MapToCourseDto).ToList();

        return new PagedResult<CourseDto>(
            courseDtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdAsync(id, cancellationToken);
        return course != null ? MapToCourseDto(course) : null;
    }

    public async Task<CourseDetailDto> GetCourseDetailAsync(
        Guid courseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdWithReviewsAsync(courseId, cancellationToken);

        if (course == null)
        {
            throw new KeyNotFoundException("课程不存在");
        }

        var reviews = await _reviewRepository.GetByCourseIdAsync(courseId, cancellationToken);
        var ratingDistribution = _reviewService.GetRatingDistribution(reviews);

        var reviewDtos = reviews.Select(MapToReviewDto).ToList();

        ReviewDto? userReview = null;
        var hasUserReviewed = false;

        if (userId.HasValue)
        {
            var userReviewEntity = await _reviewRepository.GetByUserAndCourseAsync(
                userId.Value, courseId, cancellationToken);
            if (userReviewEntity != null)
            {
                hasUserReviewed = true;
                userReview = MapToReviewDto(userReviewEntity);
            }
        }

        return new CourseDetailDto(
            Course: MapToCourseDto(course),
            Reviews: reviewDtos,
            RatingDistribution: ratingDistribution,
            HasUserReviewed: hasUserReviewed,
            UserReview: userReview);
    }

    public async Task<CourseDto> CreateCourseAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Credits < 1 || request.Credits > 10)
        {
            throw new ArgumentException("学分必须在1到10之间");
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TeacherName = request.TeacherName.Trim(),
            Semester = request.Semester.Trim(),
            Credits = request.Credits,
            CreatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course, cancellationToken);
        await _courseRepository.SaveChangesAsync(cancellationToken);

        return MapToCourseDto(course);
    }

    public async Task DeleteCourseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdAsync(id, cancellationToken);

        if (course == null)
        {
            throw new KeyNotFoundException("课程不存在");
        }

        await _courseRepository.DeleteAsync(course, cancellationToken);
        await _courseRepository.SaveChangesAsync(cancellationToken);
    }

    private static CourseDto MapToCourseDto(Course course)
    {
        return new CourseDto(
            Id: course.Id,
            Name: course.Name,
            TeacherName: course.TeacherName,
            Semester: course.Semester,
            Credits: course.Credits,
            AverageRating: course.AverageRating,
            ReviewCount: course.ReviewCount,
            CreatedAt: course.CreatedAt);
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
