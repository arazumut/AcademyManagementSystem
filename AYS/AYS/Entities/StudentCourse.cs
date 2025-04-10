namespace AYS.Entities;

public class StudentCourse : BaseEntity
{
    public string StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public decimal? FinalGrade { get; set; }
    public string Status { get; set; } // Active, Completed, Dropped
    
    // Navigation Properties
    public virtual ApplicationUser Student { get; set; }
    public virtual Course Course { get; set; }
    public virtual ICollection<ExamResult> ExamResults { get; set; }

    public StudentCourse()
    {
        ExamResults = new HashSet<ExamResult>();
        Status = "Active";
    }
} 