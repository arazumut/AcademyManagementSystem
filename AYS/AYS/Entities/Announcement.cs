namespace AYS.Models.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string AuthorId { get; set; }
    public int? CourseId { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
    public string Priority { get; set; } // Low, Medium, High
    
    // Navigation Properties
    public virtual ApplicationUser Author { get; set; }
    public virtual Course Course { get; set; }
} 