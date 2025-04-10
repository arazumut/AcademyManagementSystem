namespace AYS.Entities;

public class ExamResult : BaseEntity
{
    public int ExamId { get; set; }
    public string StudentId { get; set; }
    public int StudentCourseId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmissionDate { get; set; }
    public bool IsGraded { get; set; }
    
    // Navigation Properties
    public virtual Exam Exam { get; set; }
    public virtual ApplicationUser Student { get; set; }
    public virtual StudentCourse StudentCourse { get; set; }
} 