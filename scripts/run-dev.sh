#!/bin/bash

# TaskManager Development Script
# This script starts both the .NET API and Vue.js frontend in development mode

set -e  # Exit on any error

echo "🚀 Starting TaskManager in Development Mode"
echo "============================================="

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

# Function to check if a port is in use
port_in_use() {
    if command_exists lsof; then
        lsof -i :$1 >/dev/null 2>&1
    elif command_exists netstat; then
        netstat -an | grep :$1 >/dev/null 2>&1
    else
        return 1
    fi
}

# Function to kill process on port
kill_port() {
    if command_exists lsof; then
        local pids=$(lsof -ti :$1)
        if [ ! -z "$pids" ]; then
            print_color $YELLOW "Killing existing process on port $1..."
            echo $pids | xargs kill -9
            sleep 2
        fi
    fi
}

# Check prerequisites
print_color $BLUE "Checking prerequisites..."

if ! command_exists dotnet; then
    print_color $RED "❌ .NET SDK not found. Please install .NET 8 SDK."
    exit 1
fi

if ! command_exists node; then
    print_color $RED "❌ Node.js not found. Please install Node.js 18+."
    exit 1
fi

if ! command_exists npm; then
    print_color $RED "❌ npm not found. Please install npm."
    exit 1
fi

print_color $GREEN "✅ All prerequisites found"

# Get the script directory and project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
API_DIR="$PROJECT_ROOT/src/TaskManager.Api"
WEB_DIR="$PROJECT_ROOT/src/TaskManager.Web"

print_color $BLUE "Project root: $PROJECT_ROOT"

# Check if directories exist
if [ ! -d "$API_DIR" ]; then
    print_color $RED "❌ API directory not found: $API_DIR"
    exit 1
fi

if [ ! -d "$WEB_DIR" ]; then
    print_color $RED "❌ Web directory not found: $WEB_DIR"
    exit 1
fi

# Kill existing processes on our ports
print_color $YELLOW "Checking for existing processes..."
kill_port 7001  # API port
kill_port 5173  # Vite dev server port

# Setup database
print_color $BLUE "Setting up database..."
cd "$PROJECT_ROOT"

if [ -f "database/schema/01_create_schema.sql" ]; then
    print_color $GREEN "✅ Database scripts found"
else
    print_color $YELLOW "⚠️  Database scripts not found, database will be auto-created"
fi

# Install frontend dependencies
print_color $BLUE "Installing frontend dependencies..."
cd "$WEB_DIR"

if [ ! -d "node_modules" ] || [ "package.json" -nt "node_modules" ]; then
    print_color $YELLOW "Installing npm packages..."
    npm install
    if [ $? -ne 0 ]; then
        print_color $RED "❌ Failed to install npm packages"
        exit 1
    fi
    print_color $GREEN "✅ Frontend dependencies installed"
else
    print_color $GREEN "✅ Frontend dependencies up to date"
fi

# Restore backend dependencies
print_color $BLUE "Restoring backend dependencies..."
cd "$API_DIR"

dotnet restore
if [ $? -ne 0 ]; then
    print_color $RED "❌ Failed to restore .NET packages"
    exit 1
fi
print_color $GREEN "✅ Backend dependencies restored"

# Function to start the API
start_api() {
    print_color $BLUE "Starting .NET API..."
    cd "$API_DIR"
    
    # Set environment variables for performance
    export ASPNETCORE_ENVIRONMENT=Development
    export ASPNETCORE_URLS="https://localhost:7001;http://localhost:5001"
    export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
    
    # Start the API with performance optimizations
    dotnet run &
    API_PID=$!
    
    print_color $GREEN "✅ API started (PID: $API_PID)"
    print_color $YELLOW "   API URL: https://localhost:7001"
    print_color $YELLOW "   Swagger: https://localhost:7001/swagger"
    print_color $YELLOW "   Health: https://localhost:7001/health"
    
    # Wait for API to be ready
    print_color $BLUE "Waiting for API to be ready..."
    for i in {1..30}; do
        # Try HTTPS first, then HTTP as fallback
        if curl -k -s https://localhost:7001/health >/dev/null 2>&1; then
            print_color $GREEN "✅ API is ready on HTTPS!"
            break
        elif curl -s http://localhost:5001/health >/dev/null 2>&1; then
            print_color $GREEN "✅ API is ready on HTTP!"
            print_color $YELLOW "⚠️  Using HTTP instead of HTTPS"
            break
        fi
        if [ $i -eq 30 ]; then
            print_color $RED "❌ API failed to start within 30 seconds"
            exit 1
        fi
        sleep 1
        printf "."
    done
    echo
}

# Function to start the frontend
start_frontend() {
    print_color $BLUE "Starting Vue.js frontend..."
    cd "$WEB_DIR"
    
    # Set environment variables
    export VITE_API_BASE_URL="https://localhost:7001/api"
    
    # Start the frontend
    npm run dev &
    FRONTEND_PID=$!
    
    print_color $GREEN "✅ Frontend started (PID: $FRONTEND_PID)"
    print_color $YELLOW "   Frontend URL: http://localhost:5173"
    
    # Wait for frontend to be ready
    print_color $BLUE "Waiting for frontend to be ready..."
    for i in {1..20}; do
        if curl -s http://localhost:5173 >/dev/null 2>&1; then
            print_color $GREEN "✅ Frontend is ready!"
            break
        fi
        if [ $i -eq 20 ]; then
            print_color $YELLOW "⚠️  Frontend may still be starting..."
            break
        fi
        sleep 1
        printf "."
    done
    echo
}

# Function to cleanup on exit
cleanup() {
    print_color $YELLOW "\n🛑 Shutting down TaskManager..."
    
    if [ ! -z "$API_PID" ]; then
        print_color $YELLOW "Stopping API (PID: $API_PID)..."
        kill $API_PID 2>/dev/null || true
    fi
    
    if [ ! -z "$FRONTEND_PID" ]; then
        print_color $YELLOW "Stopping Frontend (PID: $FRONTEND_PID)..."
        kill $FRONTEND_PID 2>/dev/null || true
    fi
    
    # Kill any remaining processes on our ports
    kill_port 7001
    kill_port 5173
    
    print_color $GREEN "✅ Cleanup completed"
}

# Set up cleanup trap
trap cleanup EXIT INT TERM

# Start services
start_api
sleep 2  # Give API a moment to start
start_frontend

# Display information
print_color $GREEN "\n🎉 TaskManager is now running!"
print_color $BLUE "=============================="
print_color $YELLOW "Frontend: http://localhost:5173"
print_color $YELLOW "API:      https://localhost:7001"
print_color $YELLOW "Swagger:  https://localhost:7001/swagger"
print_color $YELLOW "Health:   https://localhost:7001/health"
print_color $BLUE "=============================="
print_color $BLUE "Press Ctrl+C to stop all services"
print_color $BLUE "Logs will appear below:"
echo

# Wait for processes to complete
wait

