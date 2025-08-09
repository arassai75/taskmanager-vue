using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO for returning task data to clients
/// </summary>
public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public int Priority { get; set; }
    public string PriorityText { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(1, 3, ErrorMessage = "Priority must be 1 (Low), 2 (Medium), or 3 (High)")]
    public int Priority { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Validates and trims the input data
    /// </summary>
    public void Normalize()
    {
        Title = Title?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
    }
}

/// <summary>
/// DTO for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(1, 3, ErrorMessage = "Priority must be 1 (Low), 2 (Medium), or 3 (High)")]
    public int Priority { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int? CategoryId { get; set; }

    /// <summary>
    /// Validates and trims the input data
    /// </summary>
    public void Normalize()
    {
        Title = Title?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
    }
}

/// <summary>
/// DTO for toggling task completion status
/// </summary>
public class ToggleTaskDto
{
    public bool IsCompleted { get; set; }
}

/// <summary>
/// DTO for task search and filtering
/// </summary>
public class TaskSearchDto
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? SearchTerm { get; set; }

    public bool? IsCompleted { get; set; }

    [Range(1, 3, ErrorMessage = "Priority must be 1 (Low), 2 (Medium), or 3 (High)")]
    public int? Priority { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int? CategoryId { get; set; }

    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be positive")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Normalizes search parameters
    /// </summary>
    public void Normalize()
    {
        SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim();
    }
}

/// <summary>
/// DTO for paginated task results
/// </summary>
public class PagedTasksDto
{
    public List<TaskDto> Tasks { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// DTO for task statistics
/// </summary>
public class TaskStatisticsDto
{
    public string Category { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int HighPriorityPending { get; set; }
    public double CompletionPercentage { get; set; }
}

/// <summary>
/// DTO for bulk operations
/// </summary>
public class BulkTaskOperationDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one task ID is required")]
    public List<int> TaskIds { get; set; } = new();
}

/// <summary>
/// DTO for bulk update operations
/// </summary>
public class BulkUpdateTaskDto : BulkTaskOperationDto
{
    public bool? IsCompleted { get; set; }

    [Range(1, 3, ErrorMessage = "Priority must be 1 (Low), 2 (Medium), or 3 (High)")]
    public int? Priority { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category ID must be a positive number")]
    public int? CategoryId { get; set; }
}

