using AYS.Data;
using AYS.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AYS.Controllers;

public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Course
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Semester)
            .ToListAsync();
        
        return View(courses);
    }

    // GET: Course/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Semester)
            .Include(c => c.EnrolledStudents)
                .ThenInclude(s => s.Student)
            .Include(c => c.Exams)
            .Include(c => c.Announcements)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // GET: Course/Create
    [Authorize(Roles = "Admin,Educator")]
    public IActionResult Create()
    {
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
        ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name");
        
        return View();
    }

    // POST: Course/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Educator")]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Description,CategoryId,SemesterId,StartDate,EndDate,IsActive,MaxStudents")] Course course)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            course.InstructorId = user.Id;
            course.CreatedBy = user.Id;
            course.CreatedAt = DateTime.UtcNow;
            
            _context.Add(course);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
        ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", course.SemesterId);
        
        return View(course);
    }

    // GET: Course/Edit/5
    [Authorize(Roles = "Admin,Educator")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        
        // Check if user is authorized to edit this course
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        
        if (!isAdmin && course.InstructorId != user.Id)
        {
            return Forbid();
        }
        
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
        ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", course.SemesterId);
        
        return View(course);
    }

    // POST: Course/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Educator")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Description,CategoryId,SemesterId,StartDate,EndDate,IsActive,MaxStudents,InstructorId,CreatedAt,CreatedBy")] Course course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        // Check if user is authorized to edit this course
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        
        if (!isAdmin && course.InstructorId != user.Id)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                course.UpdatedBy = user.Id;
                course.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", course.CategoryId);
        ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name", course.SemesterId);
        
        return View(course);
    }

    // GET: Course/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Semester)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: Course/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            course.UpdatedBy = _userManager.GetUserId(User);
            
            _context.Update(course);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }

    // GET: Course/Enroll/5
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Enroll(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (course == null)
        {
            return NotFound();
        }

        // Check if student already enrolled
        var userId = _userManager.GetUserId(User);
        var alreadyEnrolled = await _context.StudentCourses
            .AnyAsync(sc => sc.CourseId == id && sc.StudentId == userId);
            
        if (alreadyEnrolled)
        {
            TempData["Message"] = "Zaten bu derse kayıtlısınız.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Check if course is full
        var enrolledStudentsCount = await _context.StudentCourses
            .CountAsync(sc => sc.CourseId == id);
            
        if (course.MaxStudents > 0 && enrolledStudentsCount >= course.MaxStudents)
        {
            TempData["Error"] = "Ders kontenjanı dolu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(course);
    }

    // POST: Course/EnrollConfirm/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> EnrollConfirm(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        // Check if course is full
        var enrolledStudentsCount = await _context.StudentCourses
            .CountAsync(sc => sc.CourseId == id);
            
        if (course.MaxStudents > 0 && enrolledStudentsCount >= course.MaxStudents)
        {
            TempData["Error"] = "Ders kontenjanı dolu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User);
        
        // Check if student already enrolled
        var alreadyEnrolled = await _context.StudentCourses
            .AnyAsync(sc => sc.CourseId == id && sc.StudentId == userId);
            
        if (alreadyEnrolled)
        {
            TempData["Message"] = "Zaten bu derse kayıtlısınız.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Create new enrollment
        var enrollment = new StudentCourse
        {
            StudentId = userId,
            CourseId = id,
            EnrollmentDate = DateTime.UtcNow,
            Status = "Enrolled",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Add(enrollment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Derse başarıyla kaydoldunuz.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }
} 