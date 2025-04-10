using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AYS.Data;
using AYS.Entities;
using AYS.Models.ViewModels;
using System.Security.Claims;

namespace AYS.Controllers;

[Authorize(Roles = "Admin,Educator")]
public class ExamController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExamController> _logger;

    public ExamController(ApplicationDbContext context, ILogger<ExamController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Exam
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Admin can see all exams, educators can see only their own
        var exams = User.IsInRole("Admin") 
            ? await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Instructor)
                .ToListAsync()
            : await _context.Exams
                .Include(e => e.Course)
                .Include(e => e.Instructor)
                .Where(e => e.InstructorId == userId)
                .ToListAsync();

        return View(exams);
    }

    // GET: Exam/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .Include(e => e.Results)
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null)
        {
            return NotFound();
        }

        // Only admin or the instructor who created the exam can see details
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }

        return View(exam);
    }

    // GET: Exam/Create
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Admin can create exams for any course, educators only for their courses
        var coursesQuery = User.IsInRole("Admin")
            ? _context.Courses
            : _context.Courses.Where(c => c.InstructorId == userId);
            
        ViewData["CourseId"] = new SelectList(await coursesQuery.ToListAsync(), "Id", "Name");
        
        return View();
    }

    // POST: Exam/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,CourseId,ExamDate,Duration,TotalPoints,ExamType")] Exam exam)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Check if the user has rights to create an exam for this course
        var course = await _context.Courses.FindAsync(exam.CourseId);
        if (course == null)
        {
            return NotFound();
        }
        
        if (!User.IsInRole("Admin") && course.InstructorId != userId)
        {
            return Forbid();
        }
        
        exam.InstructorId = userId;
        exam.CreatedAt = DateTime.UtcNow;
        exam.IsPublished = false;
        
        if (ModelState.IsValid)
        {
            _context.Add(exam);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exam created: {Title} for Course: {CourseId}", exam.Title, exam.CourseId);
            return RedirectToAction(nameof(Index));
        }
        
        ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // GET: Exam/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can edit
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // POST: Exam/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CourseId,ExamDate,Duration,TotalPoints,ExamType,IsPublished")] Exam exam)
    {
        if (id != exam.Id)
        {
            return NotFound();
        }
        
        // Get the existing exam to check permissions and preserve instructor ID
        var existingExam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (existingExam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can edit
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && existingExam.InstructorId != userId)
        {
            return Forbid();
        }
        
        // Preserve the original InstructorId
        exam.InstructorId = existingExam.InstructorId;
        exam.CreatedAt = existingExam.CreatedAt;
        exam.UpdatedAt = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Exam updated: {Title}", exam.Title);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(exam.Id))
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
        
        ViewData["CourseId"] = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // GET: Exam/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams
            .Include(e => e.Course)
            .Include(e => e.Instructor)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (exam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can delete
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }

        return View(exam);
    }

    // POST: Exam/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can delete
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Exam deleted: {Title}", exam.Title);
        
        return RedirectToAction(nameof(Index));
    }
    
    // POST: Exam/Publish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can publish
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        exam.IsPublished = true;
        exam.UpdatedAt = DateTime.UtcNow;
        
        _context.Update(exam);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Exam published: {Title}", exam.Title);
        
        return RedirectToAction(nameof(Details), new { id = exam.Id });
    }

    private bool ExamExists(int id)
    {
        return _context.Exams.Any(e => e.Id == id);
    }
} 