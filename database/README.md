# TaskManager Database Management

This directory contains database-related files and documentation for the TaskManager application.

## Overview

The TaskManager application now uses **EF Core Migrations** for proper database versioning and deployment management. This ensures that schema changes are tracked, versioned, and can be deployed consistently across different environments.

## Directory Structure

```
database/
├── README.md              # This file
├── backups/               # Automated database backups
└── schema/                # Legacy SQL scripts (kept for reference)
    ├── 01_create_schema.sql
    └── 02_v1.1.0_schema_updates.sql
```

## Database Environments

- **Development**: `taskmanager-dev.db` (used when ASPNETCORE_ENVIRONMENT=Development)
- **Production**: `taskmanager.db` (used when ASPNETCORE_ENVIRONMENT=Production)

## Migration System

### Current Migrations

1. **20250809214103_InitialCreate** - Initial database schema with all v1.1.0 features
   - Tasks table with DueDate, EstimatedHours, NotificationsEnabled
   - Categories table with color support
   - Proper indexes and constraints
   - Sample seed data

### EF Core Migration Files

Migrations are stored in `src/TaskManager.Api/Migrations/`:
- `*.cs` - Migration code files
- `*Designer.cs` - Migration metadata
- `TaskContextModelSnapshot.cs` - Current model snapshot

## Database Deployment Script

Use the `deploy-db.sh` script for all database operations:

### Basic Commands

```bash
# Check migration status
./deploy-db.sh dev status
./deploy-db.sh prod status

# Apply pending migrations
./deploy-db.sh dev migrate
./deploy-db.sh prod migrate

# Create database backup
./deploy-db.sh dev backup
./deploy-db.sh prod backup

# Sync development schema to production
./deploy-db.sh prod sync

# Reset database (WARNING: Data loss!)
./deploy-db.sh dev reset
./deploy-db.sh prod reset

# Rollback last migration
./deploy-db.sh dev rollback
./deploy-db.sh prod rollback
```

### Deployment Workflow

1. **Development Changes**:
   ```bash
   # After modifying models, create migration
   cd src/TaskManager.Api
   dotnet ef migrations add YourMigrationName
   
   # Apply to development
   ./deploy-db.sh dev migrate
   ```

2. **Production Deployment**:
   ```bash
   # Sync development schema to production
   ./deploy-db.sh prod sync
   
   # OR apply specific migrations
   ./deploy-db.sh prod migrate
   ```

### Safety Features

- **Automatic Backups**: All operations that modify data create timestamped backups
- **Confirmation Prompts**: Destructive operations require explicit confirmation
- **Environment Isolation**: Clear separation between dev and prod databases
- **Migration Tracking**: Full audit trail of schema changes

## Schema Changes

When making schema changes:

1. **Modify the EF Core models** in `src/TaskManager.Api/Models/`
2. **Update the DbContext** in `src/TaskManager.Api/Data/TaskContext.cs` if needed
3. **Create a migration**:
   ```bash
   cd src/TaskManager.Api
   dotnet ef migrations add YourChangeDescription
   ```
4. **Test in development**:
   ```bash
   ./deploy-db.sh dev migrate
   ```
5. **Deploy to production**:
   ```bash
   ./deploy-db.sh prod sync
   ```

## Database Schema

### Current Tables

#### Tasks
- **Core Fields**: Id, Title, Description, IsCompleted, Priority
- **Categorization**: CategoryId (FK to Categories)
- **v1.1.0 Features**: DueDate, EstimatedHours, NotificationsEnabled
- **Audit**: CreatedAt, UpdatedAt, IsDeleted, DeletedAt

#### Categories
- **Core Fields**: Id, Name, Description, Color, IsActive
- **Audit**: CreatedAt

### Constraints and Indexes

- **Performance Indexes**: Optimized for common query patterns
- **Data Validation**: Check constraints for data integrity
- **Foreign Keys**: Referential integrity with cascade options

## Troubleshooting

### Common Issues

1. **Migration Conflicts**:
   ```bash
   # Check current status
   ./deploy-db.sh dev status
   
   # Reset if needed
   ./deploy-db.sh dev reset
   ```

2. **Schema Sync Issues**:
   ```bash
   # Force sync from development
   ./deploy-db.sh prod sync
   ```

3. **Backup and Restore**:
   ```bash
   # Manual backup
   ./deploy-db.sh prod backup
   
   # Restore from backup
   cp database/backups/prod_TIMESTAMP.db src/TaskManager.Api/taskmanager.db
   ```

### Database Files

- Development: `src/TaskManager.Api/taskmanager-dev.db`
- Production: `src/TaskManager.Api/taskmanager.db`
- Backups: `database/backups/`

## Migration History

| Version | Migration | Description |
|---------|-----------|-------------|
| v1.0.0 | InitialCreate | Initial schema with basic task management |
| v1.1.0 | InitialCreate | Added DueDate, EstimatedHours, NotificationsEnabled |

## Best Practices

1. **Always backup** before making schema changes
2. **Test migrations** in development first
3. **Use descriptive names** for migrations
4. **Keep migrations small** and focused
5. **Never modify existing migrations** that have been deployed
6. **Use the deployment script** for all database operations

## Legacy Files

The `database/schema/` directory contains legacy SQL scripts that were used before implementing EF Core migrations. These are kept for reference but should not be used for new deployments.