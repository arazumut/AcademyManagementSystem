namespace AYS.Models.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int? ParentCategoryId { get; set; }
    
    // Navigation Properties
    public virtual Category ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; }
    public virtual ICollection<Course> Courses { get; set; }

    public Category()
    {
        SubCategories = new HashSet<Category>();
        Courses = new HashSet<Course>();
    }
} 