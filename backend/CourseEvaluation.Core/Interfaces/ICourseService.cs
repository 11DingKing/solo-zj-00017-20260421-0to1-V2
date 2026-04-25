using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;

namespace CourseEvaluation.Core.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseDto>> GetCoursesAsync(
        CourseListRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDto?> GetCourseByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CourseDetailDto> GetCourseDetailAsync(
        Guid courseId,
        Guid? userId = null,
        CancellationToken cancellationToken = default);

    Task<CourseDto> CreateCourseAsync(
        CreateCourseRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteCourseAsync(Guid id, CancellationToken cancellationToken = default);
}
