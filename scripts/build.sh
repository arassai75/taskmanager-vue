#!/bin/bash

# TaskManager Production Build Script
# This script builds both the .NET API and Vue.js frontend for production deployment

set -e  # Exit on any error

echo " Building TaskManager for Production"
echo "======================================="

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

if ! command_exists dotnet; then
    print_color $RED ".NET SDK not found. Please install .NET 8 SDK."
    exit 1
fi

if ! command_exists node; then
    print_color $RED "Node.js not found. Please install Node.js 18+."
    exit 1
fi

if ! command_exists npm; then
    print_color $RED "npm not found. Please install npm."
    exit 1
fi

print_color $GREEN "All prerequisites found"

# Get the script directory and project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
API_DIR="$PROJECT_ROOT/src/TaskManager.Api"
WEB_DIR="$PROJECT_ROOT/src/TaskManager.Web"
BUILD_DIR="$PROJECT_ROOT/build"

print_color $BLUE "Project root: $PROJECT_ROOT"

# Create build directory
print_color $BLUE "Creating build directory..."
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"

# Build frontend
print_color $BLUE "Building Vue.js frontend..."
cd "$WEB_DIR"

# Install dependencies
print_color $YELLOW "Installing frontend dependencies..."
npm ci --only=production

# Set production environment variables
export VITE_API_BASE_URL="/api"
export NODE_ENV=production

# Build frontend
print_color $YELLOW "Building frontend assets..."
npm run build

if [ $? -ne 0 ]; then
    print_color $RED "Frontend build failed"
    exit 1
fi

# Copy frontend build to output directory
print_color $YELLOW "Copying frontend build..."
cp -r dist "$BUILD_DIR/wwwroot"

print_color $GREEN "Frontend build completed"

# Build backend
print_color $BLUE "Building .NET API..."
cd "$API_DIR"

# Restore dependencies
print_color $YELLOW "Restoring backend dependencies..."
dotnet restore

# Build for production
print_color $YELLOW "Building API for production..."
dotnet publish -c Release -o "$BUILD_DIR/api" --no-restore

if [ $? -ne 0 ]; then
    print_color $RED "Backend build failed"
    exit 1
fi

print_color $GREEN "Backend build completed"

# Copy additional files
print_color $BLUE "Copying additional files..."

# Copy database scripts
if [ -d "$PROJECT_ROOT/database" ]; then
    cp -r "$PROJECT_ROOT/database" "$BUILD_DIR/"
    print_color $GREEN "Database scripts copied"
fi

# Copy documentation
if [ -f "$PROJECT_ROOT/README.md" ]; then
    cp "$PROJECT_ROOT/README.md" "$BUILD_DIR/"
fi

# Create deployment scripts
print_color $BLUE "Creating deployment scripts..."

# Create start script for production
cat > "$BUILD_DIR/start.sh" << 'EOF'
#!/bin/bash

# TaskManager Production Start Script

echo "Starting TaskManager in Production Mode"

# Set production environment
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:5000"

# Navigate to API directory
cd "$(dirname "$0")/api"

# Start the application
exec dotnet TaskManager.Api.dll
EOF

chmod +x "$BUILD_DIR/start.sh"

# Create Windows start script
cat > "$BUILD_DIR/start.bat" << 'EOF'
@echo off
echo Starting TaskManager in Production Mode...

set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://localhost:5000

cd /d "%~dp0\api"
dotnet TaskManager.Api.dll

pause
EOF

# Create Docker files
print_color $BLUE "Creating Docker configuration..."

# Dockerfile
cat > "$BUILD_DIR/Dockerfile" << 'EOF'
# TaskManager Production Docker Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the published API
COPY api/ ./

# Copy the frontend assets
COPY wwwroot/ ./wwwroot/

# Copy database scripts
COPY database/ ./database/

# Expose port
EXPOSE 80
EXPOSE 443

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Start the application
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
EOF

# Docker Compose
cat > "$BUILD_DIR/docker-compose.yml" << 'EOF'
version: '3.8'

services:
  taskmanager:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/taskmanager.db
    volumes:
      - taskmanager_data:/app/data
    restart: unless-stopped

volumes:
  taskmanager_data:
EOF

print_color $GREEN "Docker configuration created"

# Create deployment README
cat > "$BUILD_DIR/DEPLOYMENT.md" << 'EOF'
# TaskManager Deployment Guide

## Quick Start

### Option 1: Direct Execution
```bash
# Linux/macOS
./start.sh

# Windows
start.bat
```

### Option 2: Docker
```bash
# Build and run with Docker Compose
docker-compose up -d

# Or build and run manually
docker build -t taskmanager .
docker run -p 8080:80 taskmanager
```

## Configuration

### Database
- The application uses SQLite by default
- Database file will be created automatically
- Location: `taskmanager.db` (configurable via connection string)

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Production`
- `ASPNETCORE_URLS`: Configure ports (default: `http://localhost:5000`)
- `ConnectionStrings__DefaultConnection`: Database connection string

### Frontend
- Built frontend is included in the API under `/wwwroot`
- No separate web server required
- Access the application at the configured API URL

## URLs
- Application: http://localhost:5000
- API Documentation: http://localhost:5000/swagger
- Health Check: http://localhost:5000/health

## Database Setup
Initial database setup is automatic. For manual setup:
```bash
# Navigate to database directory
cd database

# Execute schema creation
sqlite3 ../taskmanager.db < schema/01_create_schema.sql
sqlite3 ../taskmanager.db < stored_procedures/sp_task_operations.sql
```

## Performance Considerations
- Enable HTTPS in production
- Configure proper logging
- Set up reverse proxy (nginx/Apache) if needed
- Monitor database size and performance

## Security
- Change default connection strings
- Configure CORS for your domain
- Enable HTTPS
- Set up proper authentication (future enhancement)
EOF

print_color $GREEN "Deployment documentation created"

# Generate build info
print_color $BLUE "Generating build information..."

cat > "$BUILD_DIR/build-info.json" << EOF
{
  "version": "1.0.0",
  "buildDate": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "gitCommit": "$(git rev-parse HEAD 2>/dev/null || echo 'unknown')",
  "gitBranch": "$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo 'unknown')",
  "buildEnvironment": {
    "nodeVersion": "$(node --version)",
    "npmVersion": "$(npm --version)",
    "dotnetVersion": "$(dotnet --version)"
  },
  "components": {
    "api": {
      "framework": ".NET 8",
      "outputPath": "./api"
    },
    "frontend": {
      "framework": "Vue 3",
      "outputPath": "./wwwroot"
    }
  }
}
EOF

# Calculate build size
print_color $BLUE "Calculating build size..."
BUILD_SIZE=$(du -sh "$BUILD_DIR" | cut -f1)

# Display build summary
print_color $GREEN "\n🎉 Build completed successfully!"
print_color $BLUE "=================================="
print_color $YELLOW "Build output:     $BUILD_DIR"
print_color $YELLOW "Build size:       $BUILD_SIZE"
print_color $YELLOW "API location:     $BUILD_DIR/api"
print_color $YELLOW "Frontend assets:  $BUILD_DIR/wwwroot"
print_color $YELLOW "Start script:     $BUILD_DIR/start.sh"
print_color $YELLOW "Docker config:    $BUILD_DIR/Dockerfile"
print_color $BLUE "=================================="

print_color $BLUE "\nNext steps:"
print_color $YELLOW "1. Navigate to: cd $BUILD_DIR"
print_color $YELLOW "2. Start application: ./start.sh"
print_color $YELLOW "3. Or deploy with Docker: docker-compose up -d"
print_color $YELLOW "4. Access at: http://localhost:5000"

print_color $GREEN "\n TaskManager production build ready for deployment!"
EOF

