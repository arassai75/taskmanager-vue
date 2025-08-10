using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Models;

/// <summary>
/// Represents a task category for organizing tasks
/// </summary>
[Table("Categories")]
public class Category
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Category name - required and unique
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional category description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Hex color code for the category (e.g., #FF5733)
    /// </summary>
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF5733)")]
    public string? Color { get; set; }

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of tasks that belong to this category
    /// </summary>
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    /// <summary>
    /// Count of active tasks in this category
    /// </summary>
    [NotMapped]
    public int ActiveTaskCount => Tasks?.Count(t => !t.IsDeleted) ?? 0;

    /// <summary>
    /// Count of completed tasks in this category
    /// </summary>
    [NotMapped]
    public int CompletedTaskCount => Tasks?.Count(t => !t.IsDeleted && t.IsCompleted) ?? 0;

    /// <summary>
    /// Completion percentage for this category
    /// </summary>
    [NotMapped]
    public double CompletionPercentage
    {
        get
        {
            var activeCount = ActiveTaskCount;
            return activeCount == 0 ? 0 : Math.Round((double)CompletedTaskCount / activeCount * 100, 2);
        }
    }
}

