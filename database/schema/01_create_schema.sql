-- =============================================
-- TaskManager Database Schema Creation Script
-- Description: Creates the initial database structure for the TaskManager application
-- =============================================

-- Enable foreign key constraints (SQLite specific)
PRAGMA foreign_keys = ON;

-- =============================================
-- Table: Tasks
-- Description: Main table for storing task information
-- Features: Soft delete, audit trails, performance indexes
-- =============================================

CREATE TABLE IF NOT EXISTS Tasks (
    -- Primary Key
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Core Task Fields
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    IsCompleted BOOLEAN NOT NULL DEFAULT 0,
    
    -- Priority and Category (for future enhancements)
    Priority INTEGER NOT NULL DEFAULT 1, -- 1=Low, 2=Medium, 3=High
    CategoryId INTEGER NULL, -- For future category support
    
    -- Audit Fields
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Soft Delete Support
    IsDeleted BOOLEAN NOT NULL DEFAULT 0,
    DeletedAt DATETIME NULL,
    
    -- Data Validation Constraints
    CONSTRAINT CK_Tasks_Title_NotEmpty CHECK (LENGTH(TRIM(Title)) > 0),
    CONSTRAINT CK_Tasks_Priority_Valid CHECK (Priority IN (1, 2, 3)),
    CONSTRAINT CK_Tasks_DeletedAt_Logic CHECK (
        (IsDeleted = 0 AND DeletedAt IS NULL) OR 
        (IsDeleted = 1 AND DeletedAt IS NOT NULL)
    )
);

-- =============================================
-- Performance Indexes
-- Description: Optimized indexes for common query patterns
-- =============================================

-- Index for filtering active tasks (most common query)
CREATE INDEX IF NOT EXISTS IX_Tasks_IsDeleted_CreatedAt 
ON Tasks(IsDeleted, CreatedAt DESC);

-- Index for completion status filtering
CREATE INDEX IF NOT EXISTS IX_Tasks_IsCompleted_IsDeleted 
ON Tasks(IsCompleted, IsDeleted);

-- Index for text search (future enhancement)
CREATE INDEX IF NOT EXISTS IX_Tasks_Title 
ON Tasks(Title);

-- Composite index for complex queries
CREATE INDEX IF NOT EXISTS IX_Tasks_Status_Priority 
ON Tasks(IsCompleted, Priority, IsDeleted);

-- =============================================
-- Categories Table (Future Enhancement)
-- Description: Support for task categorization
-- =============================================

CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Color NVARCHAR(7) NULL, -- Hex color code (#FFFFFF)
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT CK_Categories_Name_NotEmpty CHECK (LENGTH(TRIM(Name)) > 0),
    CONSTRAINT CK_Categories_Color_Format CHECK (
        Color IS NULL OR 
        (LENGTH(Color) = 7 AND Color LIKE '#%')
    )
);

-- =============================================
-- Default Categories
-- Description: Insert default categories for immediate use
-- =============================================

INSERT OR IGNORE INTO Categories (Id, Name, Description, Color) VALUES 
(1, 'General', 'General tasks without specific category', '#6B7280'),
(2, 'Work', 'Work-related tasks and projects', '#3B82F6'),
(3, 'Personal', 'Personal tasks and reminders', '#10B981'),
(4, 'Urgent', 'High priority urgent tasks', '#EF4444');

-- =============================================
-- Sample Data (for development/demo)
-- Description: Insert sample tasks for testing
-- =============================================

INSERT OR IGNORE INTO Tasks (Id, Title, Description, IsCompleted, Priority, CategoryId) VALUES 
(1, 'Complete project documentation', 'Write comprehensive documentation for the TaskManager API', 0, 2, 2),
(2, 'Review pull requests', 'Review and approve pending pull requests in the repository', 0, 2, 2),
(3, 'Buy groceries', 'Weekly grocery shopping - milk, bread, fruits', 0, 1, 3),
(4, 'Schedule dentist appointment', 'Annual dental checkup appointment', 0, 2, 3),
(5, 'Deploy to production', 'Deploy the latest version to production environment', 1, 3, 2);

-- =============================================
-- Views for Common Queries
-- Description: Pre-defined views for frequently used data
-- =============================================

-- Active tasks with category information
CREATE VIEW IF NOT EXISTS ActiveTasksWithCategories AS
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.IsCompleted,
    t.Priority,
    t.CreatedAt,
    t.UpdatedAt,
    c.Name AS CategoryName,
    c.Color AS CategoryColor
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0;

-- Task statistics view
CREATE VIEW IF NOT EXISTS TaskStatistics AS
SELECT 
    COUNT(*) AS TotalTasks,
    SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) AS CompletedTasks,
    SUM(CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END) AS PendingTasks,
    SUM(CASE WHEN Priority = 3 AND IsCompleted = 0 THEN 1 ELSE 0 END) AS HighPriorityPending
FROM Tasks 
WHERE IsDeleted = 0;

-- =============================================
-- Triggers for Audit Trail
-- Description: Automatically update audit fields
-- =============================================

-- Update the UpdatedAt timestamp on any update
CREATE TRIGGER IF NOT EXISTS TR_Tasks_UpdatedAt
AFTER UPDATE ON Tasks
FOR EACH ROW
BEGIN
    UPDATE Tasks 
    SET UpdatedAt = CURRENT_TIMESTAMP 
    WHERE Id = NEW.Id;
END;

-- Set DeletedAt when soft deleting
CREATE TRIGGER IF NOT EXISTS TR_Tasks_SoftDelete
AFTER UPDATE ON Tasks
FOR EACH ROW
WHEN NEW.IsDeleted = 1 AND OLD.IsDeleted = 0
BEGIN
    UPDATE Tasks 
    SET DeletedAt = CURRENT_TIMESTAMP 
    WHERE Id = NEW.Id;
END;

-- =============================================
-- Schema Validation
-- Description: Verify the schema was created correctly
-- =============================================

-- Check if all tables exist
SELECT 
    name AS TableName,
    sql AS CreateStatement
FROM sqlite_master 
WHERE type = 'table' 
AND name IN ('Tasks', 'Categories')
ORDER BY name;

-- Check if all indexes exist
SELECT 
    name AS IndexName,
    tbl_name AS TableName
FROM sqlite_master 
WHERE type = 'index' 
AND name LIKE 'IX_%'
ORDER BY tbl_name, name;

-- =============================================
-- Script Completion Message
-- =============================================

SELECT 'Database schema created successfully!' AS Status,
       datetime('now') AS CompletedAt;

