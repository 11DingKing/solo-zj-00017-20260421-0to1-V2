using CourseEvaluation.Core.DTOs;

namespace CourseEvaluation.Core.Interfaces;

public interface IFavoriteService
{
    Task<bool> ToggleFavoriteAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<bool> IsFavoriteAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CourseDto>> GetFavoriteCoursesAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
