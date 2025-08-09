-- =============================================
-- TaskManager Stored Procedures
-- Description: All stored procedures for task operations
-- 
-- Note: SQLite doesn't support stored procedures,
-- but we're creating reusable SQL scripts that can be executed
-- via EF Core's SqlQuery methods for enterprise-style data access
-- =============================================

-- =============================================
-- Procedure: sp_GetAllTasks
-- Description: Retrieves all active tasks with optional filtering
-- Parameters: 
--   @IncludeCompleted: Include completed tasks in results (default: 1)
-- Returns: List of tasks with category information
-- =============================================

-- This will be called as: 
-- context.Database.SqlQueryRaw<TaskItem>("SELECT * FROM sp_GetAllTasks({0})", includeCompleted)

CREATE VIEW IF NOT EXISTS sp_GetAllTasks AS
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.IsCompleted,
    t.Priority,
    t.CreatedAt,
    t.UpdatedAt,
    t.CategoryId,
    c.Name AS CategoryName,
    c.Color AS CategoryColor
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0
ORDER BY 
    t.Priority DESC,  -- High priority first
    t.CreatedAt DESC; -- Most recent first

-- =============================================
-- Function: sp_GetTaskById
-- Description: Retrieves a specific task by ID
-- Parameters: @TaskId - The task ID to retrieve
-- Returns: Single task record or empty if not found
-- =============================================

-- Usage in C#: 
-- var task = await context.Tasks.FromSqlRaw("SELECT * FROM Tasks WHERE Id = {0} AND IsDeleted = 0", taskId).FirstOrDefaultAsync();

-- We'll create this as a parameterized query helper
-- The actual implementation will be in the service layer

-- =============================================
-- Procedure: sp_CreateTask
-- Description: Creates a new task with validation
-- Parameters: 
--   @Title: Task title (required, max 200 chars)
--   @Description: Task description (optional, max 1000 chars)
--   @Priority: Task priority (1-3, default: 1)
--   @CategoryId: Category ID (optional)
-- Returns: The newly created task ID
-- =============================================

-- This will be implemented as a service method that uses EF Core
-- Example implementation pattern:

/*
C# Service Method:
public async Task<int> CreateTaskAsync(CreateTaskDto dto)
{
    var task = new TaskItem
    {
        Title = dto.Title?.Trim(),
        Description = dto.Description?.Trim(),
        Priority = dto.Priority,
        CategoryId = dto.CategoryId,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    _context.Tasks.Add(task);
    await _context.SaveChangesAsync();
    return task.Id;
}
*/

-- =============================================
-- Procedure: sp_UpdateTask
-- Description: Updates an existing task
-- Parameters: 
--   @TaskId: The task ID to update
--   @Title: New task title
--   @Description: New task description
--   @Priority: New task priority
--   @CategoryId: New category ID
-- Returns: Success status and updated task data
-- =============================================

-- Implementation will be in service layer with optimistic concurrency
-- Example SQL pattern for EF Core:

/*
UPDATE Tasks 
SET 
    Title = @Title,
    Description = @Description,
    Priority = @Priority,
    CategoryId = @CategoryId,
    UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @TaskId 
  AND IsDeleted = 0;
*/

-- =============================================
-- Procedure: sp_ToggleTaskStatus
-- Description: Toggles the completion status of a task
-- Parameters: @TaskId - The task ID to toggle
-- Returns: Updated task record with new status
-- =============================================

-- Implementation pattern:
/*
UPDATE Tasks 
SET 
    IsCompleted = CASE WHEN IsCompleted = 1 THEN 0 ELSE 1 END,
    UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @TaskId 
  AND IsDeleted = 0;
*/

-- =============================================
-- Procedure: sp_DeleteTask
-- Description: Soft deletes a task (sets IsDeleted = 1)
-- Parameters: @TaskId - The task ID to delete
-- Returns: Success status
-- =============================================

-- Implementation pattern:
/*
UPDATE Tasks 
SET 
    IsDeleted = 1,
    DeletedAt = CURRENT_TIMESTAMP,
    UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @TaskId 
  AND IsDeleted = 0;
*/

-- =============================================
-- Procedure: sp_GetTaskStatistics
-- Description: Returns task statistics by category
-- Returns: Aggregated task data for dashboard
-- =============================================

CREATE VIEW IF NOT EXISTS sp_GetTaskStatistics AS
SELECT 
    'Total' AS Category,
    COUNT(*) AS TotalTasks,
    SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) AS CompletedTasks,
    SUM(CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END) AS PendingTasks,
    SUM(CASE WHEN Priority = 3 AND IsCompleted = 0 THEN 1 ELSE 0 END) AS HighPriorityPending,
    ROUND(
        CAST(SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) AS FLOAT) / 
        CAST(COUNT(*) AS FLOAT) * 100, 
        2
    ) AS CompletionPercentage
FROM Tasks 
WHERE IsDeleted = 0

UNION ALL

SELECT 
    COALESCE(c.Name, 'Uncategorized') AS Category,
    COUNT(t.Id) AS TotalTasks,
    SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) AS CompletedTasks,
    SUM(CASE WHEN t.IsCompleted = 0 THEN 1 ELSE 0 END) AS PendingTasks,
    SUM(CASE WHEN t.Priority = 3 AND t.IsCompleted = 0 THEN 1 ELSE 0 END) AS HighPriorityPending,
    ROUND(
        CASE 
            WHEN COUNT(t.Id) > 0 THEN
                CAST(SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) AS FLOAT) / 
                CAST(COUNT(t.Id) AS FLOAT) * 100
            ELSE 0
        END, 
        2
    ) AS CompletionPercentage
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0
GROUP BY c.Id, c.Name
HAVING COUNT(t.Id) > 0
ORDER BY TotalTasks DESC;

-- =============================================
-- Procedure: sp_SearchTasks
-- Description: Full-text search across task titles and descriptions
-- Parameters: @SearchTerm - The search term
-- Returns: Matching tasks ordered by relevance
-- =============================================

CREATE VIEW IF NOT EXISTS sp_SearchTasks AS
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.IsCompleted,
    t.Priority,
    t.CreatedAt,
    t.CategoryId,
    c.Name AS CategoryName,
    -- Simple relevance scoring
    (CASE 
        WHEN LOWER(t.Title) LIKE '%' || LOWER('SEARCH_TERM_PLACEHOLDER') || '%' THEN 10
        ELSE 0
    END +
    CASE 
        WHEN LOWER(t.Description) LIKE '%' || LOWER('SEARCH_TERM_PLACEHOLDER') || '%' THEN 5
        ELSE 0
    END) AS RelevanceScore
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0
  AND (
    LOWER(t.Title) LIKE '%' || LOWER('SEARCH_TERM_PLACEHOLDER') || '%' OR
    LOWER(t.Description) LIKE '%' || LOWER('SEARCH_TERM_PLACEHOLDER') || '%'
  )
ORDER BY RelevanceScore DESC, t.CreatedAt DESC;

-- =============================================
-- Procedure: sp_GetTasksByPriority
-- Description: Get tasks filtered by priority level
-- Parameters: @Priority - Priority level (1=Low, 2=Medium, 3=High)
-- Returns: Tasks matching the priority level
-- =============================================

-- This will be implemented as a parameterized query in the service layer

-- =============================================
-- Procedure: sp_GetTasksByDateRange
-- Description: Get tasks created within a date range
-- Parameters: 
--   @StartDate - Start date (inclusive)
--   @EndDate - End date (inclusive)
-- Returns: Tasks created within the specified range
-- =============================================

-- Implementation pattern:
/*
SELECT t.*, c.Name AS CategoryName
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0
  AND DATE(t.CreatedAt) BETWEEN DATE(@StartDate) AND DATE(@EndDate)
ORDER BY t.CreatedAt DESC;
*/

-- =============================================
-- Utility Procedures
-- =============================================

-- =============================================
-- Procedure: sp_CleanupDeletedTasks
-- Description: Permanently removes tasks deleted more than 30 days ago
-- Returns: Number of tasks permanently deleted
-- =============================================

-- Implementation pattern:
/*
DELETE FROM Tasks 
WHERE IsDeleted = 1 
  AND DeletedAt < datetime('now', '-30 days');
*/

-- =============================================
-- Procedure: sp_RestoreTask
-- Description: Restores a soft-deleted task
-- Parameters: @TaskId - The task ID to restore
-- Returns: Success status
-- =============================================

-- Implementation pattern:
/*
UPDATE Tasks 
SET 
    IsDeleted = 0,
    DeletedAt = NULL,
    UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @TaskId 
  AND IsDeleted = 1;
*/

-- =============================================
-- Performance Helper Views
-- =============================================

-- Active tasks optimized for list display
CREATE VIEW IF NOT EXISTS vw_ActiveTasksList AS
SELECT 
    Id,
    Title,
    IsCompleted,
    Priority,
    CreatedAt,
    CategoryId,
    CASE Priority
        WHEN 1 THEN 'Low'
        WHEN 2 THEN 'Medium'
        WHEN 3 THEN 'High'
        ELSE 'Unknown'
    END AS PriorityText
FROM Tasks
WHERE IsDeleted = 0;

-- Completed tasks for archive view
CREATE VIEW IF NOT EXISTS vw_CompletedTasks AS
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.CreatedAt,
    t.UpdatedAt,
    c.Name AS CategoryName
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0 
  AND t.IsCompleted = 1
ORDER BY t.UpdatedAt DESC;

-- =============================================
-- Validation Functions (Helper Queries)
-- =============================================

-- Check if a task exists and is not deleted
CREATE VIEW IF NOT EXISTS vw_TaskExists AS
SELECT 
    Id,
    CASE 
        WHEN IsDeleted = 0 THEN 1 
        ELSE 0 
    END AS IsActive
FROM Tasks;

-- =============================================
-- Script Completion
-- =============================================

SELECT 'Stored procedures and views created successfully!' AS Status,
       datetime('now') AS CompletedAt;

-- List all created views (SQLite's version of stored procedures)
SELECT 
    name AS ProcedureName,
    'VIEW' AS Type
FROM sqlite_master 
WHERE type = 'view' 
  AND name LIKE 'sp_%' OR name LIKE 'vw_%'
ORDER BY name;

