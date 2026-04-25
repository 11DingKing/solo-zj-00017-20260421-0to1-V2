using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;

namespace CourseEvaluation.Core.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<PagedResult<Course>> SearchAndSortAsync(
        CourseListRequest request,
        CancellationToken cancellationToken = default);

    Task<Course?> GetByIdWithReviewsAsync(Guid id, CancellationToken cancellationToken = default);
}
