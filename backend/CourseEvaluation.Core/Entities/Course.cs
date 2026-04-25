namespace CourseEvaluation.Core.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public double? AverageRating => Reviews.Any() 
        ? Math.Round(Reviews.Average(r => r.Rating), 2) 
        : null;

    public int ReviewCount => Reviews.Count;
}
