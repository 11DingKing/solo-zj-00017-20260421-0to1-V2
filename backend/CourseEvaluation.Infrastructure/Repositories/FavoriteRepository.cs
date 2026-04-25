using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseEvaluation.Infrastructure.Repositories;

public class FavoriteRepository : Repository<Favorite>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Favorite?> GetByUserAndCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                f => f.UserId == userId && f.CourseId == courseId,
                cancellationToken);
    }

    public async Task<PagedResult<Course>> GetFavoriteCoursesAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(f => f.Course)
                .ThenInclude(c => c.Reviews)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Course);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(items, totalCount, page, pageSize);
    }

    public async Task<List<Guid>> GetFavoriteCourseIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.UserId == userId)
            .Select(f => f.CourseId)
            .ToListAsync(cancellationToken);
    }
}
