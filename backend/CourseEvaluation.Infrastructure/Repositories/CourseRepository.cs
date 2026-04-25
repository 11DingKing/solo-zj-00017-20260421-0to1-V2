using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseEvaluation.Infrastructure.Repositories;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Course>> SearchAndSortAsync(
        CourseListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(c => c.Reviews).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search) 
                || c.TeacherName.ToLower().Contains(search));
        }

        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.SortDescending 
                ? query.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0)
                : query.OrderBy(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
            "name" => request.SortDescending 
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "reviewcount" => request.SortDescending 
                ? query.OrderByDescending(c => c.Reviews.Count)
                : query.OrderBy(c => c.Reviews.Count),
            _ => request.SortDescending 
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<Course?> GetByIdWithReviewsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
