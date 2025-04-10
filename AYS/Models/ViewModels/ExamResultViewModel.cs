using System.ComponentModel.DataAnnotations;
using AYS.Entities;

namespace AYS.Models.ViewModels;

public class ExamResultViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Sınav seçilmelidir.")]
    [Display(Name = "Sınav")]
    public int ExamId { get; set; }
    
    [Required(ErrorMessage = "Öğrenci seçilmelidir.")]
    [Display(Name = "Öğrenci")]
    public string StudentId { get; set; }
    
    [Required(ErrorMessage = "Öğrenci-Ders kaydı seçilmelidir.")]
    [Display(Name = "Öğrenci Ders Kaydı")]
    public int StudentCourseId { get; set; }
    
    [Required(ErrorMessage = "Puan girilmelidir.")]
    [Range(0, 100, ErrorMessage = "Puan 0 ile 100 arasında olmalıdır.")]
    [Display(Name = "Puan")]
    public decimal Score { get; set; }
    
    [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olmalıdır.")]
    [Display(Name = "Yorum")]
    public string? Feedback { get; set; }
    
    // Navigation Properties for displaying in views
    public Exam Exam { get; set; }
    public ApplicationUser Student { get; set; }
    public StudentCourse StudentCourse { get; set; }
} 