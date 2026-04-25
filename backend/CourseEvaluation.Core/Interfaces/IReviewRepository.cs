using CourseEvaluation.Core.Entities;

namespace CourseEvaluation.Core.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByUserAndCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<bool> HasUserReviewedCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<List<Review>> GetByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, int>> GetRatingDistributionAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<double?> CalculateAverageRatingAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
}
