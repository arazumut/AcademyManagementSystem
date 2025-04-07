namespace AYS.Models.Entities;

public class Exam : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int CourseId { get; set; }
    public string InstructorId { get; set; }
    public DateTime ExamDate { get; set; }
    public int Duration { get; set; } // in minutes
    public decimal TotalPoints { get; set; }
    public string ExamType { get; set; } // Midterm, Final, Quiz, etc.
    public bool IsPublished { get; set; }
    
    // Navigation Properties
    public virtual Course Course { get; set; }
    public virtual ApplicationUser Instructor { get; set; }
    public virtual ICollection<ExamResult> Results { get; set; }

    public Exam()
    {
        Results = new HashSet<ExamResult>();
    }
} 