#!/bin/bash

# TaskManager Database Setup Script
# This script sets up the SQLite database with schema and stored procedures

set -e  # Exit on any error

echo "🗄️  Setting up TaskManager Database"
echo "===================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_color() {
    printf "${1}${2}${NC}\n"
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
print_color $BLUE "Checking prerequisites..."

if ! command_exists sqlite3; then
    print_color $RED "❌ SQLite3 not found. Please install SQLite3."
    print_color $YELLOW "   On macOS: brew install sqlite"
    print_color $YELLOW "   On Ubuntu: sudo apt-get install sqlite3"
    print_color $YELLOW "   On Windows: Download from https://sqlite.org/download.html"
    exit 1
fi

print_color $GREEN "✅ SQLite3 found"

# Get the script directory and project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DATABASE_DIR="$PROJECT_ROOT/database"
API_DIR="$PROJECT_ROOT/src/TaskManager.Api"

print_color $BLUE "Project root: $PROJECT_ROOT"

# Check if database scripts exist
if [ ! -d "$DATABASE_DIR" ]; then
    print_color $RED "❌ Database directory not found: $DATABASE_DIR"
    exit 1
fi

if [ ! -f "$DATABASE_DIR/schema/01_create_schema.sql" ]; then
    print_color $RED "❌ Schema file not found: $DATABASE_DIR/schema/01_create_schema.sql"
    exit 1
fi

if [ ! -f "$DATABASE_DIR/stored_procedures/sp_task_operations.sql" ]; then
    print_color $RED "❌ Stored procedures file not found: $DATABASE_DIR/stored_procedures/sp_task_operations.sql"
    exit 1
fi

# Determine database locations
DEV_DB="$API_DIR/taskmanager-dev.db"
PROD_DB="$API_DIR/taskmanager.db"

# Parse command line arguments
ENVIRONMENT="development"
FORCE_RECREATE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -f|--force)
            FORCE_RECREATE=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -e, --environment    Target environment (development|production) [default: development]"
            echo "  -f, --force          Force recreation of existing database"
            echo "  -h, --help           Show this help message"
            exit 0
            ;;
        *)
            print_color $RED "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Set database path based on environment
if [ "$ENVIRONMENT" = "production" ]; then
    DB_PATH="$PROD_DB"
    print_color $BLUE "Setting up PRODUCTION database"
else
    DB_PATH="$DEV_DB"
    print_color $BLUE "Setting up DEVELOPMENT database"
fi

print_color $YELLOW "Database path: $DB_PATH"

# Check if database already exists
if [ -f "$DB_PATH" ]; then
    if [ "$FORCE_RECREATE" = true ]; then
        print_color $YELLOW "Removing existing database..."
        rm "$DB_PATH"
    else
        print_color $YELLOW "⚠️  Database already exists at: $DB_PATH"
        echo -n "Do you want to recreate it? (y/N): "
        read -r response
        if [[ "$response" =~ ^[Yy]$ ]]; then
            rm "$DB_PATH"
            print_color $GREEN "✅ Existing database removed"
        else
            print_color $BLUE "Skipping database creation"
            exit 0
        fi
    fi
fi

# Ensure API directory exists
mkdir -p "$(dirname "$DB_PATH")"

print_color $BLUE "Creating database and executing schema..."

# Execute schema creation
print_color $YELLOW "📋 Creating tables and indexes..."
sqlite3 "$DB_PATH" < "$DATABASE_DIR/schema/01_create_schema.sql"

if [ $? -ne 0 ]; then
    print_color $RED "❌ Failed to create database schema"
    exit 1
fi

print_color $GREEN "✅ Database schema created successfully"

# Execute stored procedures (views in SQLite)
print_color $YELLOW "📊 Creating views and procedures..."
sqlite3 "$DB_PATH" < "$DATABASE_DIR/stored_procedures/sp_task_operations.sql"

if [ $? -ne 0 ]; then
    print_color $RED "❌ Failed to create stored procedures"
    exit 1
fi

print_color $GREEN "✅ Views and procedures created successfully"

# Verify database structure
print_color $BLUE "Verifying database structure..."

# Check tables
TABLES=$(sqlite3 "$DB_PATH" "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;")
TABLE_COUNT=$(echo "$TABLES" | wc -l)

print_color $YELLOW "Tables created ($TABLE_COUNT):"
echo "$TABLES" | sed 's/^/  - /'

# Check views
VIEWS=$(sqlite3 "$DB_PATH" "SELECT name FROM sqlite_master WHERE type='view' ORDER BY name;")
VIEW_COUNT=$(echo "$VIEWS" | wc -l)

print_color $YELLOW "Views created ($VIEW_COUNT):"
echo "$VIEWS" | sed 's/^/  - /'

# Check indexes
INDEXES=$(sqlite3 "$DB_PATH" "SELECT name FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%' ORDER BY name;")
INDEX_COUNT=$(echo "$INDEXES" | wc -l)

print_color $YELLOW "Indexes created ($INDEX_COUNT):"
echo "$INDEXES" | sed 's/^/  - /'

# Check sample data
TASK_COUNT=$(sqlite3 "$DB_PATH" "SELECT COUNT(*) FROM Tasks;")
CATEGORY_COUNT=$(sqlite3 "$DB_PATH" "SELECT COUNT(*) FROM Categories;")

print_color $YELLOW "Sample data:"
print_color $YELLOW "  - Tasks: $TASK_COUNT"
print_color $YELLOW "  - Categories: $CATEGORY_COUNT"

# Database statistics
DB_SIZE=$(du -h "$DB_PATH" | cut -f1)
print_color $YELLOW "Database size: $DB_SIZE"

# Test basic operations
print_color $BLUE "Testing basic database operations..."

# Test task retrieval
TEST_QUERY="SELECT COUNT(*) as total, 
                   SUM(CASE WHEN IsCompleted = 1 THEN 1 ELSE 0 END) as completed,
                   SUM(CASE WHEN IsCompleted = 0 THEN 1 ELSE 0 END) as pending
            FROM Tasks WHERE IsDeleted = 0;"

STATS=$(sqlite3 "$DB_PATH" "$TEST_QUERY")
print_color $GREEN "✅ Database query test successful"
print_color $YELLOW "Task statistics: $STATS"

# Create a backup for production environments
if [ "$ENVIRONMENT" = "production" ]; then
    BACKUP_PATH="$DB_PATH.backup-$(date +%Y%m%d-%H%M%S)"
    cp "$DB_PATH" "$BACKUP_PATH"
    print_color $GREEN "✅ Database backup created: $BACKUP_PATH"
fi

# Set appropriate permissions
chmod 644 "$DB_PATH"
print_color $GREEN "✅ Database permissions set"

# Display connection information
print_color $GREEN "\n🎉 Database setup completed successfully!"
print_color $BLUE "=========================================="
print_color $YELLOW "Environment:      $ENVIRONMENT"
print_color $YELLOW "Database path:    $DB_PATH"
print_color $YELLOW "Database size:    $DB_SIZE"
print_color $YELLOW "Tables:           $TABLE_COUNT"
print_color $YELLOW "Views:            $VIEW_COUNT"
print_color $YELLOW "Indexes:          $INDEX_COUNT"
print_color $YELLOW "Sample tasks:     $TASK_COUNT"
print_color $YELLOW "Categories:       $CATEGORY_COUNT"
print_color $BLUE "=========================================="

print_color $BLUE "\nConnection string for appsettings.json:"
print_color $YELLOW "\"DefaultConnection\": \"Data Source=$DB_PATH\""

print_color $BLUE "\nTest database connection:"
print_color $YELLOW "sqlite3 $DB_PATH"
print_color $YELLOW ".tables"
print_color $YELLOW ".quit"

print_color $GREEN "\n✅ Database is ready for use!"

