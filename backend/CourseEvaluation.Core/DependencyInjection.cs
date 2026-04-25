using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CourseEvaluation.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
