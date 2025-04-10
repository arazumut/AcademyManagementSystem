namespace AYS.Entities;

public class Semester : BaseEntity
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Course> Courses { get; set; }

    public Semester()
    {
        Courses = new HashSet<Course>();
    }
} 