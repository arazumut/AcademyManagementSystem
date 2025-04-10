using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AYS.Data;
using AYS.Models;
using AYS.Models.ViewModels;
using System.Security.Claims;

namespace AYS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = new DashboardViewModel();
        
        // Rol bazlı filtreleme için kullanıcı kimliğini alalım
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Temel istatistikleri yükleyelim
        dashboardData.TotalCourses = await _context.Courses.CountAsync();
        dashboardData.TotalExams = await _context.Exams.CountAsync();
        dashboardData.TotalStudents = await _context.Users.CountAsync(u => u.NormalizedEmail.EndsWith("@STUDENT.COM") || _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == "Student"));
        dashboardData.TotalEducators = await _context.Users.CountAsync(u => u.NormalizedEmail.EndsWith("@EDUCATOR.COM") || _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == "Educator"));
        
        // Yaklaşan sınavları yükleyelim
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Student"))
            {
                // Öğrencinin yaklaşan sınavları (öğrencinin kayıtlı olduğu kursların sınavları)
                dashboardData.UpcomingExams = await _context.Exams
                    .Include(e => e.Course)
                    .Where(e => e.IsPublished && 
                                e.ExamDate > DateTime.Now && 
                                _context.StudentCourses.Any(sc => sc.StudentId == userId && sc.CourseId == e.CourseId))
                    .OrderBy(e => e.ExamDate)
                    .Take(5)
                    .ToListAsync();
            }
            else if (User.IsInRole("Educator"))
            {
                // Eğitmenin yaklaşan sınavları
                dashboardData.UpcomingExams = await _context.Exams
                    .Include(e => e.Course)
                    .Where(e => e.InstructorId == userId && e.ExamDate > DateTime.Now)
                    .OrderBy(e => e.ExamDate)
                    .Take(5)
                    .ToListAsync();
                
                // Eğitmenin kursları
                dashboardData.MyCourses = await _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .Include(c => c.EnrolledStudents)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            else if (User.IsInRole("Admin"))
            {
                // Admin için tüm yaklaşan sınavlar
                dashboardData.UpcomingExams = await _context.Exams
                    .Include(e => e.Course)
                    .Where(e => e.IsPublished && e.ExamDate > DateTime.Now)
                    .OrderBy(e => e.ExamDate)
                    .Take(5)
                    .ToListAsync();
                
                // Son eklenen kurslar
                dashboardData.MyCourses = await _context.Courses
                    .Include(c => c.EnrolledStudents)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
        }
        else
        {
            // Ziyaretçiler için örnek sınavlar (yayınlanmış ve yaklaşan)
            dashboardData.UpcomingExams = await _context.Exams
                .Include(e => e.Course)
                .Where(e => e.IsPublished && e.ExamDate > DateTime.Now)
                .OrderBy(e => e.ExamDate)
                .Take(3)
                .ToListAsync();
        }
        
        // Son duyuruları getirelim
        dashboardData.RecentAnnouncements = await _context.Announcements
            .Include(a => a.Author)
            .Include(a => a.Course)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync();
        
        return View(dashboardData);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
