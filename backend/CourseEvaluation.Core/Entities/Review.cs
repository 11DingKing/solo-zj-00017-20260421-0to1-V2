namespace CourseEvaluation.Core.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}
