-- =============================================
-- TaskManager Database Schema Updates v1.1.0
-- Description: Adds support for due dates and time estimates
-- Version: 1.1.0
-- Features Added:
--   - Due Dates with optional timestamps
--   - Time Estimates in hours
--   - Notification preferences
-- =============================================

-- Enable foreign key constraints (SQLite specific)
PRAGMA foreign_keys = ON;

-- =============================================
-- Schema Updates for Tasks Table
-- Description: Add new columns for enhanced task management
-- =============================================

-- Add DueDate column (optional datetime)
ALTER TABLE Tasks 
ADD COLUMN DueDate DATETIME NULL;

-- Add EstimatedHours column (decimal hours, nullable)
ALTER TABLE Tasks 
ADD COLUMN EstimatedHours DECIMAL(6,2) NULL;

-- Add NotificationsEnabled column (per-task notification preference)
ALTER TABLE Tasks 
ADD COLUMN NotificationsEnabled BOOLEAN NOT NULL DEFAULT 1;

-- =============================================
-- Data Validation Constraints for New Columns
-- =============================================

-- Ensure EstimatedHours is positive when provided
ALTER TABLE Tasks 
ADD CONSTRAINT CK_Tasks_EstimatedHours_Positive 
CHECK (EstimatedHours IS NULL OR EstimatedHours > 0);

-- Ensure EstimatedHours has reasonable upper limit (999.99 hours)
ALTER TABLE Tasks 
ADD CONSTRAINT CK_Tasks_EstimatedHours_Reasonable 
CHECK (EstimatedHours IS NULL OR EstimatedHours <= 999.99);

-- =============================================
-- Performance Indexes for New Features
-- =============================================

-- Index for due date queries (overdue tasks, upcoming deadlines)
CREATE INDEX IF NOT EXISTS IX_Tasks_DueDate 
ON Tasks(DueDate) 
WHERE IsDeleted = 0 AND DueDate IS NOT NULL;

-- Index for tasks with estimates (reporting, analytics)
CREATE INDEX IF NOT EXISTS IX_Tasks_EstimatedHours 
ON Tasks(EstimatedHours) 
WHERE IsDeleted = 0 AND EstimatedHours IS NOT NULL;

-- Composite index for notification queries (due soon + notifications enabled)
CREATE INDEX IF NOT EXISTS IX_Tasks_Notifications 
ON Tasks(DueDate, NotificationsEnabled, IsCompleted) 
WHERE IsDeleted = 0;

-- =============================================
-- Sample Data Updates
-- Description: Add due dates and estimates to existing sample tasks
-- =============================================

-- Update existing tasks with sample due dates and estimates
UPDATE Tasks 
SET 
    DueDate = datetime('now', '+3 days'),
    EstimatedHours = 2.5,
    NotificationsEnabled = 1
WHERE Title = 'Complete project documentation';

UPDATE Tasks 
SET 
    DueDate = datetime('now', '+1 day'),
    EstimatedHours = 1.0,
    NotificationsEnabled = 1
WHERE Title = 'Review pull requests';

UPDATE Tasks 
SET 
    DueDate = datetime('now', '+7 days'),
    EstimatedHours = 0.5,
    NotificationsEnabled = 1
WHERE Title = 'Buy groceries';

UPDATE Tasks 
SET 
    DueDate = datetime('now', '+2 days'),
    EstimatedHours = 1.5,
    NotificationsEnabled = 1
WHERE Title = 'Schedule dentist appointment';

-- Mark deployed task as having no due date (already completed)
UPDATE Tasks 
SET 
    EstimatedHours = 4.0,
    NotificationsEnabled = 0
WHERE Title = 'Deploy to production';



-- =============================================
-- Updated Stored Procedures/Views
-- Description: Update views to include new columns
-- =============================================

-- Drop and recreate the task summary view with new columns
DROP VIEW IF EXISTS vw_TaskSummary;

CREATE VIEW vw_TaskSummary AS
SELECT 
    t.Id,
    t.Title,
    t.Description,
    t.IsCompleted,
    t.Priority,
    CASE t.Priority 
        WHEN 1 THEN 'Low'
        WHEN 2 THEN 'Medium' 
        WHEN 3 THEN 'High'
        ELSE 'Unknown'
    END as PriorityText,
    t.CategoryId,
    COALESCE(c.Name, 'Uncategorized') as CategoryName,
    COALESCE(c.Color, '#6B7280') as CategoryColor,
    t.DueDate,
    t.EstimatedHours,
    t.NotificationsEnabled,
    CASE 
        WHEN t.DueDate IS NULL THEN NULL
        WHEN t.DueDate < datetime('now') AND t.IsCompleted = 0 THEN 'overdue'
        WHEN t.DueDate < datetime('now', '+1 day') AND t.IsCompleted = 0 THEN 'due_soon'
        ELSE 'normal'
    END as DueStatus,
    t.CreatedAt,
    t.UpdatedAt
FROM Tasks t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.IsDeleted = 0;

-- =============================================
-- Database Schema Version Tracking
-- =============================================

CREATE TABLE IF NOT EXISTS SchemaVersions (
    Version NVARCHAR(20) PRIMARY KEY,
    AppliedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Description NVARCHAR(500) NOT NULL
);

-- Record this schema update
INSERT OR REPLACE INTO SchemaVersions (Version, Description) 
VALUES ('1.1.0', 'Added due dates, time estimates, and user preferences support');

-- =============================================
-- Verification Queries
-- Description: Verify the schema updates were applied correctly
-- =============================================

-- Verify new columns exist
PRAGMA table_info(Tasks);

-- Verify sample data was updated
SELECT 
    Title, 
    DueDate, 
    EstimatedHours, 
    NotificationsEnabled 
FROM Tasks 
WHERE DueDate IS NOT NULL;



-- Verify schema version tracking
SELECT * FROM SchemaVersions ORDER BY AppliedAt DESC;
