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
public class ExamResultController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExamResultController> _logger;

    public ExamResultController(ApplicationDbContext context, ILogger<ExamResultController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: ExamResult
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Admin tüm sonuçları görebilir, eğitmenler sadece kendi sınavlarının sonuçlarını görebilir
        var examResults = User.IsInRole("Admin") 
            ? await _context.ExamResults
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .Include(er => er.Student)
                .OrderByDescending(er => er.CreatedAt)
                .ToListAsync()
            : await _context.ExamResults
                .Include(er => er.Exam)
                    .ThenInclude(e => e.Course)
                .Include(er => er.Student)
                .Where(er => er.Exam.InstructorId == userId)
                .OrderByDescending(er => er.CreatedAt)
                .ToListAsync();

        return View(examResults);
    }

    // GET: ExamResult/ForExam/5
    public async Task<IActionResult> ForExam(int? id)
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

        // Only admin or the instructor who created the exam can see results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }

        return View(exam);
    }

    // GET: ExamResult/Create/5 (5 is the exam id)
    public async Task<IActionResult> Create(int? examId)
    {
        if (examId == null)
        {
            return NotFound();
        }

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == examId);

        if (exam == null)
        {
            return NotFound();
        }

        // Only admin or the instructor who created the exam can add results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }

        // Get students enrolled in the course
        var enrolledStudents = await _context.StudentCourses
            .Include(sc => sc.Student)
            .Where(sc => sc.CourseId == exam.CourseId)
            .ToListAsync();

        if (!enrolledStudents.Any())
        {
            TempData["ErrorMessage"] = "Bu derse kayıtlı öğrenci bulunmamaktadır.";
            return RedirectToAction("Details", "Exam", new { id = examId });
        }

        // Create a view model
        var viewModel = new ExamResultViewModel
        {
            ExamId = exam.Id
        };

        // Prepare SelectLists for dropdowns
        ViewData["StudentId"] = new SelectList(enrolledStudents, "StudentId", "Student.FirstName");
        ViewData["StudentCourseId"] = new SelectList(enrolledStudents, "Id", "Id");
        ViewData["ExamName"] = exam.Title;
        ViewData["CourseName"] = exam.Course.Name;

        return View(viewModel);
    }

    // POST: ExamResult/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamResultViewModel viewModel)
    {
        var exam = await _context.Exams.FindAsync(viewModel.ExamId);
        if (exam == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can add results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        if (ModelState.IsValid)
        {
            // Check if student is enrolled in the course
            var studentCourse = await _context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.Id == viewModel.StudentCourseId && sc.StudentId == viewModel.StudentId);
                
            if (studentCourse == null)
            {
                ModelState.AddModelError("", "Öğrenci bu derse kayıtlı değil.");
                return View(viewModel);
            }
            
            // Check if result already exists
            var existingResult = await _context.ExamResults
                .FirstOrDefaultAsync(er => er.ExamId == viewModel.ExamId && er.StudentId == viewModel.StudentId);
                
            if (existingResult != null)
            {
                ModelState.AddModelError("", "Bu öğrenci için zaten bir sınav sonucu kaydedilmiş.");
                return View(viewModel);
            }
            
            // Create the new result
            var examResult = new ExamResult
            {
                ExamId = viewModel.ExamId,
                StudentId = viewModel.StudentId,
                StudentCourseId = viewModel.StudentCourseId,
                Score = viewModel.Score,
                Feedback = viewModel.Feedback,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Add(examResult);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Exam result created for student {StudentId} in exam {ExamId}", viewModel.StudentId, viewModel.ExamId);
            
            return RedirectToAction("Details", "Exam", new { id = viewModel.ExamId });
        }
        
        // If we got this far, something failed, redisplay form
        var enrolledStudents = await _context.StudentCourses
            .Include(sc => sc.Student)
            .Where(sc => sc.CourseId == exam.CourseId)
            .ToListAsync();
            
        ViewData["StudentId"] = new SelectList(enrolledStudents, "StudentId", "Student.FirstName", viewModel.StudentId);
        ViewData["StudentCourseId"] = new SelectList(enrolledStudents, "Id", "Id", viewModel.StudentCourseId);
        ViewData["ExamName"] = exam.Title;
        ViewData["CourseName"] = (await _context.Courses.FindAsync(exam.CourseId))?.Name;
        
        return View(viewModel);
    }

    // GET: ExamResult/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var examResult = await _context.ExamResults
            .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
            .Include(er => er.Student)
            .Include(er => er.StudentCourse)
            .FirstOrDefaultAsync(er => er.Id == id);

        if (examResult == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can edit results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && examResult.Exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        // Create the view model
        var viewModel = new ExamResultViewModel
        {
            Id = examResult.Id,
            ExamId = examResult.ExamId,
            StudentId = examResult.StudentId,
            StudentCourseId = examResult.StudentCourseId,
            Score = examResult.Score,
            Feedback = examResult.Feedback,
            Exam = examResult.Exam,
            Student = examResult.Student,
            StudentCourse = examResult.StudentCourse
        };
        
        return View(viewModel);
    }

    // POST: ExamResult/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamResultViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }
        
        var examResult = await _context.ExamResults
            .Include(er => er.Exam)
            .FirstOrDefaultAsync(er => er.Id == id);
            
        if (examResult == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can edit results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && examResult.Exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                // Update only the fields that can be changed
                examResult.Score = viewModel.Score;
                examResult.Feedback = viewModel.Feedback;
                examResult.UpdatedAt = DateTime.UtcNow;
                
                _context.Update(examResult);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Exam result updated for ID {Id}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamResultExists(viewModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Details", "Exam", new { id = examResult.ExamId });
        }
        
        // Reload the related entities for the view
        viewModel.Exam = await _context.Exams.FindAsync(viewModel.ExamId);
        viewModel.Student = await _context.Users.FindAsync(viewModel.StudentId);
        viewModel.StudentCourse = await _context.StudentCourses.FindAsync(viewModel.StudentCourseId);
        
        return View(viewModel);
    }

    // GET: ExamResult/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var examResult = await _context.ExamResults
            .Include(er => er.Exam)
                .ThenInclude(e => e.Course)
            .Include(er => er.Student)
            .FirstOrDefaultAsync(er => er.Id == id);

        if (examResult == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can delete results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && examResult.Exam.InstructorId != userId)
        {
            return Forbid();
        }

        return View(examResult);
    }

    // POST: ExamResult/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var examResult = await _context.ExamResults
            .Include(er => er.Exam)
            .FirstOrDefaultAsync(er => er.Id == id);
            
        if (examResult == null)
        {
            return NotFound();
        }
        
        // Only admin or the instructor who created the exam can delete results
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && examResult.Exam.InstructorId != userId)
        {
            return Forbid();
        }
        
        var examId = examResult.ExamId;
        
        _context.ExamResults.Remove(examResult);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Exam result deleted: ID {Id}", id);
        
        return RedirectToAction("Details", "Exam", new { id = examId });
    }

    private bool ExamResultExists(int id)
    {
        return _context.ExamResults.Any(e => e.Id == id);
    }
} 