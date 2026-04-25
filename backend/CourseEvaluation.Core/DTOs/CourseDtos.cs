namespace CourseEvaluation.Core.DTOs;

public record CourseDto(
    Guid Id,
    string Name,
    string TeacherName,
    string Semester,
    int Credits,
    double? AverageRating,
    int ReviewCount,
    DateTime CreatedAt);

public record CreateCourseRequest(
    string Name,
    string TeacherName,
    string Semester,
    int Credits);

public record CourseListRequest(
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
