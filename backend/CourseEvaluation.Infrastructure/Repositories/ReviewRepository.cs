using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseEvaluation.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Review?> GetByUserAndCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> HasUserReviewedCourseAsync(
        Guid userId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.UserId == userId && r.CourseId == courseId, cancellationToken);
    }

    public async Task<List<Review>> GetByCourseIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.User)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await DbSet
            .Where(r => r.CourseId == courseId)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var distribution = new Dictionary<int, int>
        {
            { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
        };

        foreach (var item in reviews)
        {
            if (distribution.ContainsKey(item.Rating))
            {
                distribution[item.Rating] = item.Count;
            }
        }

        return distribution;
    }

    public async Task<double?> CalculateAverageRatingAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var reviews = await DbSet.Where(r => r.CourseId == courseId).ToListAsync(cancellationToken);
        
        if (!reviews.Any())
        {
            return null;
        }

        return Math.Round(reviews.Average(r => r.Rating), 2);
    }
}
