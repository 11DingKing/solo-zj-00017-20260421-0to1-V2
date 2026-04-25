using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;

namespace CourseEvaluation.Core.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ICourseRepository _courseRepository;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        ICourseRepository courseRepository)
    {
        _favoriteRepository = favoriteRepository;
        _courseRepository = courseRepository;
    }

    public async Task<bool> ToggleFavoriteAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
        if (course == null)
        {
            throw new KeyNotFoundException("课程不存在");
        }

        var existingFavorite = await _favoriteRepository.GetByUserAndCourseAsync(
            userId, courseId, cancellationToken);

        if (existingFavorite != null)
        {
            await _favoriteRepository.DeleteAsync(existingFavorite, cancellationToken);
            await _favoriteRepository.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            var favorite = new Favorite
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId,
                CreatedAt = DateTime.UtcNow
            };

            await _favoriteRepository.AddAsync(favorite, cancellationToken);
            await _favoriteRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public async Task<bool> IsFavoriteAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var favorite = await _favoriteRepository.GetByUserAndCourseAsync(
            userId, courseId, cancellationToken);
        return favorite != null;
    }

    public async Task<PagedResult<CourseDto>> GetFavoriteCoursesAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _favoriteRepository.GetFavoriteCoursesAsync(
            userId, page, pageSize, cancellationToken);

        var courseDtos = pagedResult.Items.Select(MapToCourseDto).ToList();

        return new PagedResult<CourseDto>(
            courseDtos,
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize);
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
}
