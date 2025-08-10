using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Controllers;

/// <summary>
/// REST API controller for task management operations
/// Implements enterprise-level API design patterns with comprehensive error handling
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active tasks with optional filtering
    /// </summary>
    /// <param name="includeCompleted">Whether to include completed tasks (default: true)</param>
    /// <returns>List of tasks</returns>
    /// <response code="200">Returns the list of tasks</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAllTasks(
        [FromQuery] bool includeCompleted = true)
    {
        try
        {
            _logger.LogInformation("GET /api/tasks called with includeCompleted: {IncludeCompleted}", includeCompleted);
            
            var tasks = await _taskService.GetAllTasksAsync(includeCompleted);
            
            _logger.LogInformation("Successfully retrieved {Count} tasks", tasks.Count());
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return Problem(
                title: "Error retrieving tasks",
                detail: "An error occurred while retrieving tasks from the database",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Retrieves a specific task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task details</returns>
    /// <response code="200">Returns the task</response>
    /// <response code="404">Task not found</response>
    /// <response code="400">Invalid task ID</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> GetTask(
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be a positive number")] int id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("GET /api/tasks/{Id} called", id);
            
            var task = await _taskService.GetTaskByIdAsync(id);
            
            if (task == null)
            {
                _logger.LogWarning("Task with ID {Id} not found", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = $"Task with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Successfully retrieved task with ID {Id}", id);
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {Id}", id);
            return Problem(
                title: "Error retrieving task",
                detail: $"An error occurred while retrieving task with ID {id}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="createTaskDto">The task creation data</param>
    /// <returns>The created task</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid task data</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for task creation: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("POST /api/tasks called with title: {Title}", createTaskDto.Title);
            
            var createdTask = await _taskService.CreateTaskAsync(createTaskDto);
            
            _logger.LogInformation("Successfully created task with ID {Id}", createdTask.Id);
            return CreatedAtAction(
                nameof(GetTask), 
                new { id = createdTask.Id }, 
                createdTask
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for task creation");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid task data",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Problem(
                title: "Error creating task",
                detail: "An error occurred while creating the task",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">The task ID to update</param>
    /// <param name="updateTaskDto">The updated task data</param>
    /// <returns>The updated task</returns>
    /// <response code="200">Task updated successfully</response>
    /// <response code="404">Task not found</response>
    /// <response code="400">Invalid task data</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be a positive number")] int id,
        [FromBody] UpdateTaskDto updateTaskDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for task update: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("PUT /api/tasks/{Id} called", id);
            
            var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);
            
            if (updatedTask == null)
            {
                _logger.LogWarning("Task with ID {Id} not found for update", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = $"Task with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Successfully updated task with ID {Id}", id);
            return Ok(updatedTask);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for task update");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid task data",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task with ID {Id}", id);
            return Problem(
                title: "Error updating task",
                detail: $"An error occurred while updating task with ID {id}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Toggles the completion status of a task
    /// </summary>
    /// <param name="id">The task ID to toggle</param>
    /// <returns>The updated task</returns>
    /// <response code="200">Task status toggled successfully</response>
    /// <response code="404">Task not found</response>
    /// <response code="400">Invalid task ID</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPatch("{id:int}/toggle")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> ToggleTaskCompletion(
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be a positive number")] int id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("PATCH /api/tasks/{Id}/toggle called", id);
            
            var updatedTask = await _taskService.ToggleTaskCompletionAsync(id);
            
            if (updatedTask == null)
            {
                _logger.LogWarning("Task with ID {Id} not found for toggle", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = $"Task with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Successfully toggled completion for task with ID {Id}", id);
            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling task completion for ID {Id}", id);
            return Problem(
                title: "Error toggling task completion",
                detail: $"An error occurred while toggling completion for task with ID {id}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Soft deletes a task
    /// </summary>
    /// <param name="id">The task ID to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="404">Task not found</response>
    /// <response code="400">Invalid task ID</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTask(
        [Range(1, int.MaxValue, ErrorMessage = "Task ID must be a positive number")] int id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("DELETE /api/tasks/{Id} called", id);
            
            var deleted = await _taskService.DeleteTaskAsync(id);
            
            if (!deleted)
            {
                _logger.LogWarning("Task with ID {Id} not found for deletion", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Task not found",
                    Detail = $"Task with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("Successfully deleted task with ID {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task with ID {Id}", id);
            return Problem(
                title: "Error deleting task",
                detail: $"An error occurred while deleting task with ID {id}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Searches tasks with filtering and pagination
    /// </summary>
    /// <param name="searchDto">Search criteria</param>
    /// <returns>Paginated task results</returns>
    /// <response code="200">Returns the search results</response>
    /// <response code="400">Invalid search criteria</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(PagedTasksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedTasksDto>> SearchTasks([FromBody] TaskSearchDto searchDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for task search: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("POST /api/tasks/search called");
            
            var results = await _taskService.SearchTasksAsync(searchDto);
            
            _logger.LogInformation("Successfully searched tasks, found {TotalCount} results", results.TotalCount);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tasks");
            return Problem(
                title: "Error searching tasks",
                detail: "An error occurred while searching tasks",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Gets task statistics grouped by category
    /// </summary>
    /// <returns>Task statistics</returns>
    /// <response code="200">Returns the task statistics</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(IEnumerable<TaskStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskStatisticsDto>>> GetTaskStatistics()
    {
        try
        {
            _logger.LogInformation("GET /api/tasks/statistics called");
            
            var statistics = await _taskService.GetTaskStatisticsAsync();
            
            _logger.LogInformation("Successfully retrieved task statistics");
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task statistics");
            return Problem(
                title: "Error retrieving statistics",
                detail: "An error occurred while retrieving task statistics",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Gets tasks by priority level
    /// </summary>
    /// <param name="priority">Priority level (1=Low, 2=Medium, 3=High)</param>
    /// <returns>Tasks with the specified priority</returns>
    /// <response code="200">Returns the tasks</response>
    /// <response code="400">Invalid priority level</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("priority/{priority:int}")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByPriority(
        [Range(1, 3, ErrorMessage = "Priority must be 1 (Low), 2 (Medium), or 3 (High)")] int priority)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("GET /api/tasks/priority/{Priority} called", priority);
            
            var tasks = await _taskService.GetTasksByPriorityAsync(priority);
            
            _logger.LogInformation("Successfully retrieved {Count} tasks with priority {Priority}", 
                tasks.Count(), priority);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks by priority {Priority}", priority);
            return Problem(
                title: "Error retrieving tasks by priority",
                detail: $"An error occurred while retrieving tasks with priority {priority}",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Performs bulk operations on multiple tasks
    /// </summary>
    /// <param name="bulkUpdateDto">Bulk update data</param>
    /// <returns>Number of tasks updated</returns>
    /// <response code="200">Bulk update completed successfully</response>
    /// <response code="400">Invalid bulk update data</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPatch("bulk")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BulkUpdateTasks([FromBody] BulkUpdateTaskDto bulkUpdateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for bulk update: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("PATCH /api/tasks/bulk called for {Count} tasks", bulkUpdateDto.TaskIds.Count);
            
            var updatedCount = await _taskService.BulkUpdateTasksAsync(bulkUpdateDto);
            
            _logger.LogInformation("Successfully updated {Count} tasks", updatedCount);
            return Ok(new { UpdatedCount = updatedCount, Message = $"{updatedCount} tasks updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk update");
            return Problem(
                title: "Error performing bulk update",
                detail: "An error occurred while performing bulk update on tasks",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Performs bulk delete on multiple tasks
    /// </summary>
    /// <param name="bulkOperationDto">Bulk operation data</param>
    /// <returns>Number of tasks deleted</returns>
    /// <response code="200">Bulk delete completed successfully</response>
    /// <response code="400">Invalid bulk operation data</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpDelete("bulk")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BulkDeleteTasks([FromBody] BulkTaskOperationDto bulkOperationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for bulk delete: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("DELETE /api/tasks/bulk called for {Count} tasks", bulkOperationDto.TaskIds.Count);
            
            var deletedCount = await _taskService.BulkDeleteTasksAsync(bulkOperationDto);
            
            _logger.LogInformation("Successfully deleted {Count} tasks", deletedCount);
            return Ok(new { DeletedCount = deletedCount, Message = $"{deletedCount} tasks deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk delete");
            return Problem(
                title: "Error performing bulk delete",
                detail: "An error occurred while performing bulk delete on tasks",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

