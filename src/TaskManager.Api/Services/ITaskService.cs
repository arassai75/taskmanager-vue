using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Interface for task management services
/// Defines the contract for task-related business operations
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Retrieves all active tasks with optional filtering
    /// </summary>
    /// <param name="includeCompleted">Whether to include completed tasks</param>
    /// <returns>List of task DTOs</returns>
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(bool includeCompleted = true);

    /// <summary>
    /// Retrieves a specific task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>Task DTO or null if not found</returns>
    Task<TaskDto?> GetTaskByIdAsync(int id);

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="createTaskDto">Task creation data</param>
    /// <returns>The created task DTO</returns>
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">The task ID to update</param>
    /// <param name="updateTaskDto">Updated task data</param>
    /// <returns>The updated task DTO or null if not found</returns>
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);

    /// <summary>
    /// Toggles the completion status of a task
    /// </summary>
    /// <param name="id">The task ID to toggle</param>
    /// <returns>The updated task DTO or null if not found</returns>
    Task<TaskDto?> ToggleTaskCompletionAsync(int id);

    /// <summary>
    /// Soft deletes a task
    /// </summary>
    /// <param name="id">The task ID to delete</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<bool> DeleteTaskAsync(int id);

    /// <summary>
    /// Searches tasks with filtering and pagination
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <returns>Paginated task results</returns>
    Task<PagedTasksDto> SearchTasksAsync(TaskSearchDto searchDto);

    /// <summary>
    /// Gets task statistics grouped by category
    /// </summary>
    /// <returns>List of task statistics</returns>
    Task<IEnumerable<TaskStatisticsDto>> GetTaskStatisticsAsync();

    /// <summary>
    /// Gets tasks by priority level
    /// </summary>
    /// <param name="priority">Priority level (1-3)</param>
    /// <returns>List of tasks with the specified priority</returns>
    Task<IEnumerable<TaskDto>> GetTasksByPriorityAsync(int priority);

    /// <summary>
    /// Gets tasks created within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>List of tasks created within the date range</returns>
    Task<IEnumerable<TaskDto>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Performs bulk update operations on multiple tasks
    /// </summary>
    /// <param name="bulkUpdateDto">Bulk update data</param>
    /// <returns>Number of tasks updated</returns>
    Task<int> BulkUpdateTasksAsync(BulkUpdateTaskDto bulkUpdateDto);

    /// <summary>
    /// Performs bulk delete operations on multiple tasks
    /// </summary>
    /// <param name="bulkOperationDto">Bulk operation data</param>
    /// <returns>Number of tasks deleted</returns>
    Task<int> BulkDeleteTasksAsync(BulkTaskOperationDto bulkOperationDto);

    /// <summary>
    /// Restores a soft-deleted task
    /// </summary>
    /// <param name="id">The task ID to restore</param>
    /// <returns>The restored task DTO or null if not found</returns>
    Task<TaskDto?> RestoreTaskAsync(int id);

    /// <summary>
    /// Permanently deletes tasks that have been soft-deleted for more than specified days
    /// </summary>
    /// <param name="daysOld">Number of days old for cleanup (default: 30)</param>
    /// <returns>Number of tasks permanently deleted</returns>
    Task<int> CleanupDeletedTasksAsync(int daysOld = 30);

    /// <summary>
    /// Checks if a task exists and is not deleted
    /// </summary>
    /// <param name="id">The task ID to check</param>
    /// <returns>True if the task exists and is active</returns>
    Task<bool> TaskExistsAsync(int id);
}

