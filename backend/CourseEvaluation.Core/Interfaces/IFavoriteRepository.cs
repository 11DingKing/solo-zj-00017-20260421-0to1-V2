using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;

namespace CourseEvaluation.Core.Interfaces;

public interface IFavoriteRepository : IRepository<Favorite>
{
    Task<Favorite?> GetByUserAndCourseAsync(
        Guid userId, 
        Guid courseId, 
        CancellationToken cancellationToken = default);

    Task<PagedResult<Course>> GetFavoriteCoursesAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> GetFavoriteCourseIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
