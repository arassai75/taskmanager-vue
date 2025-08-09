using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Models;

/// <summary>
/// Represents a task item in the task management system
/// </summary>
[Table("Tasks")]
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Task title - required field with maximum length of 200 characters
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional task description with maximum length of 1000 characters
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the task has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Task priority level: 1 = Low, 2 = Medium, 3 = High
    /// </summary>
    [Range(1, 3)]
    public int Priority { get; set; } = 1;

    /// <summary>
    /// Optional foreign key reference to Categories table
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Navigation property to the category
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }

    /// <summary>
    /// Timestamp when the task was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the task was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - indicates if the task has been deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Timestamp when the task was soft deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Priority as human-readable text
    /// </summary>
    [NotMapped]
    public string PriorityText => Priority switch
    {
        1 => "Low",
        2 => "Medium",
        3 => "High",
        _ => "Unknown"
    };

    /// <summary>
    /// Indicates if the task is overdue (has a due date in the past and is not completed)
    /// Note: Due date functionality can be added in future iterations
    /// </summary>
    [NotMapped]
    public bool IsOverdue => false; // Placeholder for future due date functionality

    /// <summary>
    /// Category name for display purposes
    /// </summary>
    [NotMapped]
    public string CategoryName => Category?.Name ?? "Uncategorized";

    /// <summary>
    /// Updates the UpdatedAt timestamp
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the task as soft deleted
    /// </summary>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Restores a soft deleted task
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Toggles the completion status of the task
    /// </summary>
    public void ToggleCompletion()
    {
        IsCompleted = !IsCompleted;
        MarkAsUpdated();
    }
}

