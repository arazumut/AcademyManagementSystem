using Microsoft.AspNetCore.Identity;

namespace AYS.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Course> TeachingCourses { get; set; }
    public virtual ICollection<StudentCourse> EnrolledCourses { get; set; }
    public virtual ICollection<Exam> CreatedExams { get; set; }
    public virtual ICollection<ExamResult> ExamResults { get; set; }
    public virtual ICollection<Announcement> Announcements { get; set; }

    public ApplicationUser()
    {
        TeachingCourses = new HashSet<Course>();
        EnrolledCourses = new HashSet<StudentCourse>();
        CreatedExams = new HashSet<Exam>();
        ExamResults = new HashSet<ExamResult>();
        Announcements = new HashSet<Announcement>();
    }
} 