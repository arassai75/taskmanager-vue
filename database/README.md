# Database Documentation

This directory contains all database-related scripts and documentation for the TaskManager application.

## Structure

```
database/
├── README.md                           # This file
├── schema/
│   └── 01_create_schema.sql           # Initial database schema creation
└── stored_procedures/
    └── sp_task_operations.sql         # All task-related stored procedures
```

## Database Design

### Tables

#### Tasks
- **Purpose**: Store all task information
- **Primary Key**: Id (auto-increment)
- **Features**: 
  - Supports soft delete (IsDeleted flag)
  - Tracks creation and modification timestamps
  - Optimized with indexes on frequently queried columns

### Stored Procedures

#### sp_GetAllTasks
- **Purpose**: Retrieve all active tasks with optional filtering
- **Parameters**: 
  - `@IncludeCompleted` (BIT): Include completed tasks in results
- **Returns**: All task columns ordered by CreatedAt DESC

#### sp_GetTaskById
- **Purpose**: Retrieve a specific task by ID
- **Parameters**:
  - `@TaskId` (INT): The task ID to retrieve
- **Returns**: Single task record or NULL if not found

#### sp_CreateTask
- **Purpose**: Create a new task
- **Parameters**:
  - `@Title` (NVARCHAR(200)): Task title (required)
  - `@Description` (NVARCHAR(1000)): Task description (optional)
- **Returns**: The newly created task ID

#### sp_ToggleTaskStatus
- **Purpose**: Toggle the completion status of a task
- **Parameters**:
  - `@TaskId` (INT): The task ID to toggle
- **Returns**: Updated task record

#### sp_UpdateTask
- **Purpose**: Update task details
- **Parameters**:
  - `@TaskId` (INT): The task ID to update
  - `@Title` (NVARCHAR(200)): New title
  - `@Description` (NVARCHAR(1000)): New description
- **Returns**: Updated task record

#### sp_DeleteTask
- **Purpose**: Soft delete a task (sets IsDeleted = 1)
- **Parameters**:
  - `@TaskId` (INT): The task ID to delete
- **Returns**: Success status

## Setup Instructions

1. **Create Database**: Run `01_create_schema.sql` to create the database structure
2. **Create Procedures**: Run `sp_task_operations.sql` to create all stored procedures
3. **Verify Setup**: Check that all tables and procedures exist

## Performance Considerations

- **Indexes**: Created on frequently queried columns (CreatedAt, IsCompleted, IsDeleted)
- **Soft Delete**: Uses IsDeleted flag for better performance and data recovery
- **Stored Procedures**: Optimized queries with proper parameter handling
- **Data Types**: Appropriate sizes to minimize storage overhead

## Usage in Application

The .NET application uses Entity Framework Core with stored procedure calls:

```csharp
// Example usage
var tasks = await _context.Database
    .SqlQueryRaw<TaskItem>("EXEC sp_GetAllTasks @IncludeCompleted = {0}", true)
    .ToListAsync();
```

## Benefits of This Approach

1. **Performance**: Compiled execution plans
2. **Security**: Protection against SQL injection
3. **Maintainability**: Database logic centralized in procedures
4. **Flexibility**: Easy to modify business logic without code changes
5. **Enterprise Ready**: Standard approach in enterprise applications

