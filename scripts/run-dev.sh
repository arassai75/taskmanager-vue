#!/bin/bash

# TaskManager Development Start Script
# This script starts both the .NET API and Vue.js frontend as background processes.

set -e # Exit immediately if a command exits with a non-zero status.

# --- Colors for output ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_color() {
    printf "${1}${2}${NC}\n"
}

# --- Get script's directory and project root ---
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
API_DIR="$PROJECT_ROOT/src/TaskManager.Api"
WEB_DIR="$PROJECT_ROOT/src/TaskManager.Web"
PID_DIR="$PROJECT_ROOT/.pids"
API_PID_FILE="$PID_DIR/api.pid"
FRONTEND_PID_FILE="$PID_DIR/frontend.pid"

# --- Configuration ---
API_PORT=5001
API_URL="http://localhost:$API_PORT"
FRONTEND_PORT=5173
FRONTEND_URL="http://localhost:$FRONTEND_PORT"

# --- Main script ---
print_color $BLUE "Starting TaskManager in DEV Mode"
print_color $BLUE "=========================================="

# --- Stop any existing processes first ---
if [ -f "$SCRIPT_DIR/stop-dev.sh" ]; then
    print_color $YELLOW "Attempting to stop any running services first..."
    bash "$SCRIPT_DIR/stop-dev.sh"
fi
sleep 1

# --- Create PID directory ---
mkdir -p "$PID_DIR"

# --- Start API ---
print_color $BLUE "Starting .NET API..."
cd "$API_DIR"

# Restore dependencies if needed
print_color $YELLOW "Restoring .NET dependencies..."
dotnet restore --verbosity quiet

# Start API in background using nohup
print_color $YELLOW "Launching API on $API_URL..."
nohup dotnet run --launch-profile http > api.log 2>&1 &
API_PID=$!
echo $API_PID > "$API_PID_FILE"
print_color $GREEN "API process started with PID: $API_PID"

# Wait for API to be healthy
print_color $YELLOW "Waiting for API to be ready..."
for i in {1..20}; do
    if curl -s -o /dev/null -w "%{http_code}" "$API_URL/health" | grep -q "200"; then
        print_color $GREEN "API is ready!"
        break
    fi
    if [ $i -eq 20 ]; then
        print_color $RED "API failed to start. Check logs in $API_DIR/api.log"
        # Clean up the failed process
        kill $API_PID >/dev/null 2>&1
        rm -f "$API_PID_FILE"
        exit 1
    fi
    sleep 1
    printf "."
done
echo

# --- Start Frontend ---
print_color $BLUE "Starting Vue.js Frontend..."
cd "$WEB_DIR"

# Install dependencies if node_modules doesn't exist
if [ ! -d "node_modules" ]; then
    print_color $YELLOW "Frontend dependencies not found. Running 'npm install'..."
    npm install
fi

# Start frontend in background using nohup
print_color $YELLOW "Launching frontend on $FRONTEND_URL..."
nohup npm run dev > frontend.log 2>&1 &
FRONTEND_PID=$!
echo $FRONTEND_PID > "$FRONTEND_PID_FILE"
print_color $GREEN "Frontend process started with PID: $FRONTEND_PID"

# --- Summary ---
print_color $GREEN "TaskManager is running in the background!"
print_color $BLUE "=========================================="
print_color $YELLOW "Frontend:     $FRONTEND_URL"
print_color $YELLOW "API:          $API_URL"
print_color $YELLOW "API Swagger:  $API_URL/swagger"
print_color $BLUE "=============Logs============="
print_color $YELLOW "API Log:      tail -f $API_DIR/api.log"
print_color $YELLOW "Frontend Log: tail -f $WEB_DIR/frontend.log"
print_color $BLUE "=========================================="
print_color $RED "To stop all services, run: ./scripts/stop-dev.sh"