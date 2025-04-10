namespace AYS.Entities;

public class Course : BaseEntity
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Credits { get; set; }
    public string? Syllabus { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxStudents { get; set; }
    public bool IsActive { get; set; }
    public string InstructorId { get; set; }
    public int CategoryId { get; set; }
    public int SemesterId { get; set; }
    
    // Navigation Properties
    public virtual ApplicationUser Instructor { get; set; }
    public virtual Category Category { get; set; }
    public virtual Semester Semester { get; set; }
    public virtual ICollection<StudentCourse> EnrolledStudents { get; set; }
    public virtual ICollection<Exam> Exams { get; set; }
    public virtual ICollection<Announcement> Announcements { get; set; }

    public Course()
    {
        EnrolledStudents = new HashSet<StudentCourse>();
        Exams = new HashSet<Exam>();
        Announcements = new HashSet<Announcement>();
    }
} 