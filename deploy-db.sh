#!/bin/bash

# ==============================================
# TaskManager Database Deployment Script
# Handles migration and deployment across environments
# ==============================================

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
API_DIR="src/TaskManager.Api"
BACKUP_DIR="database/backups"

echo -e "${BLUE}🚀 TaskManager Database Deployment Script${NC}"
echo "=============================================="

# Function to display help
show_help() {
    echo "Usage: $0 [ENVIRONMENT] [COMMAND]"
    echo ""
    echo "ENVIRONMENTS:"
    echo "  dev        - Development environment (taskmanager-dev.db)"
    echo "  prod       - Production environment (taskmanager.db)"
    echo ""
    echo "COMMANDS:"
    echo "  migrate    - Apply pending migrations"
    echo "  rollback   - Rollback last migration"
    echo "  reset      - Reset database (WARNING: Data loss!)"
    echo "  backup     - Create database backup"
    echo "  status     - Show migration status"
    echo "  sync       - Sync dev schema to production"
    echo ""
    echo "Examples:"
    echo "  $0 dev migrate     - Apply migrations to development"
    echo "  $0 prod sync       - Sync dev schema to production"
    echo "  $0 prod backup     - Backup production database"
    echo "  $0 dev status      - Show development migration status"
}

# Function to backup database
backup_database() {
    local env=$1
    local db_file=""
    
    if [ "$env" = "dev" ]; then
        db_file="taskmanager-dev.db"
    else
        db_file="taskmanager.db"
    fi
    
    if [ ! -f "$API_DIR/$db_file" ]; then
        echo -e "${YELLOW}⚠️  Database file $db_file not found, skipping backup${NC}"
        return 0
    fi
    
    # Create backup directory if it doesn't exist
    mkdir -p "$BACKUP_DIR"
    
    # Create backup with timestamp
    local timestamp=$(date +"%Y%m%d_%H%M%S")
    local backup_file="$BACKUP_DIR/${env}_${timestamp}.db"
    
    echo -e "${BLUE}📦 Creating backup: $backup_file${NC}"
    cp "$API_DIR/$db_file" "$backup_file"
    
    echo -e "${GREEN}✅ Backup created successfully${NC}"
}

# Function to apply migrations
apply_migrations() {
    local env=$1
    
    echo -e "${BLUE}🔄 Applying migrations for $env environment${NC}"
    
    cd "$API_DIR"
    
    if [ "$env" = "dev" ]; then
        export ASPNETCORE_ENVIRONMENT=Development
    else
        export ASPNETCORE_ENVIRONMENT=Production
    fi
    
    # Add dotnet tools to PATH
    export PATH="$PATH:$HOME/.dotnet/tools"
    
    # Check if there are pending migrations
    echo "Checking for pending migrations..."
    if dotnet ef migrations list | grep -q "No migrations"; then
        echo -e "${YELLOW}⚠️  No migrations found${NC}"
        cd - > /dev/null
        return 0
    fi
    
    # Apply migrations
    echo "Applying migrations..."
    dotnet ef database update
    
    echo -e "${GREEN}✅ Migrations applied successfully${NC}"
    cd - > /dev/null
}

# Function to show migration status
show_status() {
    local env=$1
    
    echo -e "${BLUE}📊 Migration status for $env environment${NC}"
    
    cd "$API_DIR"
    
    if [ "$env" = "dev" ]; then
        export ASPNETCORE_ENVIRONMENT=Development
    else
        export ASPNETCORE_ENVIRONMENT=Production
    fi
    
    export PATH="$PATH:$HOME/.dotnet/tools"
    
    echo "Available migrations:"
    dotnet ef migrations list
    
    echo ""
    echo "Database info:"
    dotnet ef dbcontext info
    
    cd - > /dev/null
}

# Function to sync dev to production
sync_to_production() {
    echo -e "${YELLOW}🔄 Syncing development schema to production${NC}"
    echo -e "${RED}⚠️  WARNING: This will reset the production database!${NC}"
    echo -e "${RED}⚠️  All production data will be lost!${NC}"
    echo ""
    read -p "Are you absolutely sure? (type 'YES' to continue): " confirm
    
    if [ "$confirm" != "YES" ]; then
        echo -e "${YELLOW}❌ Sync cancelled${NC}"
        return 1
    fi
    
    # Backup production first
    echo -e "${BLUE}📦 Creating production backup before sync...${NC}"
    backup_database "prod"
    
    # Remove production database
    if [ -f "$API_DIR/taskmanager.db" ]; then
        rm "$API_DIR/taskmanager.db"
        echo -e "${GREEN}✅ Production database removed${NC}"
    fi
    
    # Apply migrations to production
    apply_migrations "prod"
    
    echo -e "${GREEN}✅ Schema sync completed successfully${NC}"
}

# Function to reset database
reset_database() {
    local env=$1
    local db_file=""
    
    if [ "$env" = "dev" ]; then
        db_file="taskmanager-dev.db"
    else
        db_file="taskmanager.db"
    fi
    
    echo -e "${RED}⚠️  WARNING: This will completely reset the $env database!${NC}"
    echo -e "${RED}⚠️  All data will be lost!${NC}"
    echo ""
    read -p "Are you sure? (type 'YES' to continue): " confirm
    
    if [ "$confirm" != "YES" ]; then
        echo -e "${YELLOW}❌ Reset cancelled${NC}"
        return 1
    fi
    
    # Backup first
    backup_database "$env"
    
    # Remove database
    if [ -f "$API_DIR/$db_file" ]; then
        rm "$API_DIR/$db_file"
        echo -e "${GREEN}✅ Database file removed${NC}"
    fi
    
    # Apply migrations
    apply_migrations "$env"
    
    echo -e "${GREEN}✅ Database reset completed${NC}"
}

# Function to rollback migration
rollback_migration() {
    local env=$1
    
    echo -e "${YELLOW}🔄 Rolling back last migration for $env environment${NC}"
    
    cd "$API_DIR"
    
    if [ "$env" = "dev" ]; then
        export ASPNETCORE_ENVIRONMENT=Development
    else
        export ASPNETCORE_ENVIRONMENT=Production
    fi
    
    export PATH="$PATH:$HOME/.dotnet/tools"
    
    # Backup first
    echo "Creating backup before rollback..."
    cd - > /dev/null
    backup_database "$env"
    cd "$API_DIR"
    
    # Show current migrations
    echo "Current migrations:"
    dotnet ef migrations list
    echo ""
    
    # Get the previous migration
    local migrations=($(dotnet ef migrations list --no-build | grep -v "Build succeeded" | grep -v "^$" | tail -n +2))
    
    if [ ${#migrations[@]} -lt 2 ]; then
        echo -e "${YELLOW}⚠️  Cannot rollback - need at least 2 migrations${NC}"
        cd - > /dev/null
        return 1
    fi
    
    local target_migration=${migrations[-2]}  # Second to last migration
    
    echo "Rolling back to migration: $target_migration"
    read -p "Continue? (y/N): " confirm
    
    if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
        dotnet ef database update "$target_migration"
        echo -e "${GREEN}✅ Rollback completed${NC}"
    else
        echo -e "${YELLOW}❌ Rollback cancelled${NC}"
    fi
    
    cd - > /dev/null
}

# Main script logic
if [ $# -eq 0 ]; then
    show_help
    exit 1
fi

ENVIRONMENT=$1
COMMAND=$2

# Validate environment
if [ "$ENVIRONMENT" != "dev" ] && [ "$ENVIRONMENT" != "prod" ]; then
    echo -e "${RED}❌ Invalid environment: $ENVIRONMENT${NC}"
    echo "Must be 'dev' or 'prod'"
    exit 1
fi

# Validate command
case $COMMAND in
    migrate)
        backup_database "$ENVIRONMENT"
        apply_migrations "$ENVIRONMENT"
        ;;
    rollback)
        rollback_migration "$ENVIRONMENT"
        ;;
    reset)
        reset_database "$ENVIRONMENT"
        ;;
    backup)
        backup_database "$ENVIRONMENT"
        ;;
    status)
        show_status "$ENVIRONMENT"
        ;;
    sync)
        if [ "$ENVIRONMENT" != "prod" ]; then
            echo -e "${RED}❌ Sync can only target production environment${NC}"
            exit 1
        fi
        sync_to_production
        ;;
    *)
        echo -e "${RED}❌ Invalid command: $COMMAND${NC}"
        show_help
        exit 1
        ;;
esac

echo -e "${GREEN}🎉 Database deployment completed successfully!${NC}"
