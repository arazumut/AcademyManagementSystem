using AYS.Entities;

namespace AYS.Models.ViewModels;

public class DashboardViewModel
{
    // İstatistikler
    public int TotalCourses { get; set; }
    public int TotalExams { get; set; }
    public int TotalStudents { get; set; }
    public int TotalEducators { get; set; }
    
    // Listeler
    public IEnumerable<Exam> UpcomingExams { get; set; } = new List<Exam>();
    public IEnumerable<Course> MyCourses { get; set; } = new List<Course>();
    public IEnumerable<Announcement> RecentAnnouncements { get; set; } = new List<Announcement>();
} 