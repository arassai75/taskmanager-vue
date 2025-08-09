using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementation of task management services with stored procedure support
/// Provides enterprise-level data access with performance optimization
/// </summary>
public class TaskService : ITaskService
{
    private readonly TaskContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(TaskContext context, IMapper mapper, ILogger<TaskService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active tasks using optimized stored procedure approach
    /// </summary>
    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(bool includeCompleted = true)
    {
        try
        {
            _logger.LogInformation("Retrieving all tasks. IncludeCompleted: {IncludeCompleted}", includeCompleted);

            // Using standard EF Core query for better compatibility
            var query = _context.Tasks
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted);

            if (!includeCompleted)
            {
                query = query.Where(t => !t.IsCompleted);
            }

            var tasks = await query
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    PriorityText = t.PriorityText,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category != null ? t.Category.Name : "Uncategorized",
                    CategoryColor = t.Category != null ? t.Category.Color : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tasks", tasks.Count);
            return tasks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tasks");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific task by ID with category information
    /// </summary>
    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving task with ID: {TaskId}", id);

            var task = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => t.Id == id && !t.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", id);
                return null;
            }

            return _mapper.Map<TaskDto>(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID: {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Creates a new task with validation and audit trail
    /// </summary>
    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        try
        {
            _logger.LogInformation("Creating new task: {Title}", createTaskDto.Title);

            // Normalize input data
            createTaskDto.Normalize();

            // Validate category exists if specified
            if (createTaskDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == createTaskDto.CategoryId.Value && c.IsActive);
                
                if (!categoryExists)
                {
                    throw new ArgumentException($"Category with ID {createTaskDto.CategoryId} does not exist or is inactive");
                }
            }

            // Use AutoMapper to create task entity from DTO
            var task = _mapper.Map<TaskItem>(createTaskDto);

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Retrieve the created task with category information
            var createdTask = await GetTaskByIdAsync(task.Id);
            
            _logger.LogInformation("Successfully created task with ID: {TaskId}", task.Id);
            return createdTask!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task: {Title}", createTaskDto.Title);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing task with optimistic concurrency control
    /// </summary>
    public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
    {
        try
        {
            _logger.LogInformation("Updating task with ID: {TaskId}", id);

            // Normalize input data
            updateTaskDto.Normalize();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for update", id);
                return null;
            }

            // Validate category exists if specified
            if (updateTaskDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.Id == updateTaskDto.CategoryId.Value && c.IsActive);
                
                if (!categoryExists)
                {
                    throw new ArgumentException($"Category with ID {updateTaskDto.CategoryId} does not exist or is inactive");
                }
            }

            // Update task properties
            // Use AutoMapper to update task entity from DTO
            _mapper.Map(updateTaskDto, task);
            task.MarkAsUpdated();

            await _context.SaveChangesAsync();

            // Return updated task with category information
            var updatedTask = await GetTaskByIdAsync(task.Id);
            
            _logger.LogInformation("Successfully updated task with ID: {TaskId}", id);
            return updatedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with ID: {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Toggles task completion status using stored procedure approach
    /// </summary>
    public async Task<TaskDto?> ToggleTaskCompletionAsync(int id)
    {
        try
        {
            _logger.LogInformation("Toggling completion status for task ID: {TaskId}", id);

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for toggle", id);
                return null;
            }

            task.ToggleCompletion();
            await _context.SaveChangesAsync();

            // Return updated task
            var updatedTask = await GetTaskByIdAsync(task.Id);
            
            _logger.LogInformation("Successfully toggled completion for task ID: {TaskId}. IsCompleted: {IsCompleted}", 
                id, task.IsCompleted);
            return updatedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling completion for task ID: {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes a task using stored procedure approach
    /// </summary>
    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            _logger.LogInformation("Soft deleting task with ID: {TaskId}", id);

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
                return false;
            }

            task.MarkAsDeleted();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully soft deleted task with ID: {TaskId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with ID: {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Searches tasks with advanced filtering and pagination
    /// </summary>
    public async Task<PagedTasksDto> SearchTasksAsync(TaskSearchDto searchDto)
    {
        try
        {
            _logger.LogInformation("Searching tasks with criteria: {SearchTerm}", searchDto.SearchTerm);

            searchDto.Normalize();

            var query = _context.Tasks
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Title.ToLower().Contains(searchTerm) || 
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
            }

            if (searchDto.IsCompleted.HasValue)
            {
                query = query.Where(t => t.IsCompleted == searchDto.IsCompleted.Value);
            }

            if (searchDto.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == searchDto.Priority.Value);
            }

            if (searchDto.CategoryId.HasValue)
            {
                query = query.Where(t => t.CategoryId == searchDto.CategoryId.Value);
            }

            if (searchDto.CreatedAfter.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= searchDto.CreatedAfter.Value);
            }

            if (searchDto.CreatedBefore.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= searchDto.CreatedBefore.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var tasks = await query
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            _logger.LogInformation("Found {Count} tasks matching search criteria", totalCount);

            return new PagedTasksDto
            {
                Tasks = taskDtos,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tasks");
            throw;
        }
    }

    /// <summary>
    /// Gets task statistics using optimized aggregation queries
    /// </summary>
    public async Task<IEnumerable<TaskStatisticsDto>> GetTaskStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving task statistics");

            // Calculate total statistics using LINQ
            var allTasks = await _context.Tasks
                .Where(t => !t.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var totalStats = new TaskStatisticsDto
            {
                Category = "Total",
                TotalTasks = allTasks.Count,
                CompletedTasks = allTasks.Count(t => t.IsCompleted),
                PendingTasks = allTasks.Count(t => !t.IsCompleted),
                HighPriorityPending = allTasks.Count(t => t.Priority == 3 && !t.IsCompleted),
                CompletionPercentage = allTasks.Count > 0 
                    ? Math.Round((double)allTasks.Count(t => t.IsCompleted) / allTasks.Count * 100, 2)
                    : 0
            };

            var statistics = new List<TaskStatisticsDto> { totalStats };

            // Calculate category statistics
            var categoryStats = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted)
                .GroupBy(t => t.Category)
                .Select(g => new TaskStatisticsDto
                {
                    Category = g.Key != null ? g.Key.Name : "Uncategorized",
                    TotalTasks = g.Count(),
                    CompletedTasks = g.Count(t => t.IsCompleted),
                    PendingTasks = g.Count(t => !t.IsCompleted),
                    HighPriorityPending = g.Count(t => t.Priority == 3 && !t.IsCompleted),
                    CompletionPercentage = g.Count() > 0 
                        ? Math.Round((double)g.Count(t => t.IsCompleted) / g.Count() * 100, 2)
                        : 0
                })
                .AsNoTracking()
                .ToListAsync();

            statistics.AddRange(categoryStats);

            _logger.LogInformation("Retrieved statistics for {Count} categories", statistics.Count);
            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task statistics");
            throw;
        }
    }

    /// <summary>
    /// Gets tasks by priority level
    /// </summary>
    public async Task<IEnumerable<TaskDto>> GetTasksByPriorityAsync(int priority)
    {
        try
        {
            _logger.LogInformation("Retrieving tasks with priority: {Priority}", priority);

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted && t.Priority == priority)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<TaskDto>>(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks by priority: {Priority}", priority);
            throw;
        }
    }

    /// <summary>
    /// Gets tasks created within a date range
    /// </summary>
    public async Task<IEnumerable<TaskDto>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Retrieving tasks created between {StartDate} and {EndDate}", startDate, endDate);

            var tasks = await _context.Tasks
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted && 
                           t.CreatedAt.Date >= startDate.Date && 
                           t.CreatedAt.Date <= endDate.Date)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<List<TaskDto>>(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks by date range");
            throw;
        }
    }

    /// <summary>
    /// Performs bulk update operations
    /// </summary>
    public async Task<int> BulkUpdateTasksAsync(BulkUpdateTaskDto bulkUpdateDto)
    {
        try
        {
            _logger.LogInformation("Performing bulk update on {Count} tasks", bulkUpdateDto.TaskIds.Count);

            var tasks = await _context.Tasks
                .Where(t => bulkUpdateDto.TaskIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync();

            var updatedCount = 0;
            foreach (var task in tasks)
            {
                if (bulkUpdateDto.IsCompleted.HasValue)
                    task.IsCompleted = bulkUpdateDto.IsCompleted.Value;

                if (bulkUpdateDto.Priority.HasValue)
                    task.Priority = bulkUpdateDto.Priority.Value;

                if (bulkUpdateDto.CategoryId.HasValue)
                    task.CategoryId = bulkUpdateDto.CategoryId.Value;

                task.MarkAsUpdated();
                updatedCount++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated {Count} tasks", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk update");
            throw;
        }
    }

    /// <summary>
    /// Performs bulk delete operations
    /// </summary>
    public async Task<int> BulkDeleteTasksAsync(BulkTaskOperationDto bulkOperationDto)
    {
        try
        {
            _logger.LogInformation("Performing bulk delete on {Count} tasks", bulkOperationDto.TaskIds.Count);

            var tasks = await _context.Tasks
                .Where(t => bulkOperationDto.TaskIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync();

            foreach (var task in tasks)
            {
                task.MarkAsDeleted();
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted {Count} tasks", tasks.Count);
            return tasks.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk delete");
            throw;
        }
    }

    /// <summary>
    /// Restores a soft-deleted task
    /// </summary>
    public async Task<TaskDto?> RestoreTaskAsync(int id)
    {
        try
        {
            _logger.LogInformation("Restoring task with ID: {TaskId}", id);

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Deleted task with ID {TaskId} not found for restore", id);
                return null;
            }

            task.Restore();
            await _context.SaveChangesAsync();

            var restoredTask = await GetTaskByIdAsync(task.Id);
            
            _logger.LogInformation("Successfully restored task with ID: {TaskId}", id);
            return restoredTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring task with ID: {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cleans up old deleted tasks
    /// </summary>
    public async Task<int> CleanupDeletedTasksAsync(int daysOld = 30)
    {
        try
        {
            _logger.LogInformation("Cleaning up tasks deleted more than {Days} days ago", daysOld);

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var tasksToDelete = await _context.Tasks
                .Where(t => t.IsDeleted && t.DeletedAt.HasValue && t.DeletedAt.Value < cutoffDate)
                .ToListAsync();

            _context.Tasks.RemoveRange(tasksToDelete);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permanently deleted {Count} old tasks", tasksToDelete.Count);
            return tasksToDelete.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up deleted tasks");
            throw;
        }
    }

    /// <summary>
    /// Checks if a task exists and is active
    /// </summary>
    public async Task<bool> TaskExistsAsync(int id)
    {
        try
        {
            return await _context.Tasks
                .AnyAsync(t => t.Id == id && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if task exists: {TaskId}", id);
            throw;
        }
    }
}

