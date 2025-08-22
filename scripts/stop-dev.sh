#!/bin/bash

# TaskManager Development Stop Script
# This script stops the background processes started by run-dev.sh.

set -e

# --- Colors for output ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_color() {
    printf "${1}${2}${NC}\n"
}

# --- Get script's directory and project root ---
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
PID_DIR="$PROJECT_ROOT/.pids"
API_PID_FILE="$PID_DIR/api.pid"
FRONTEND_PID_FILE="$PID_DIR/frontend.pid"

print_color $YELLOW "Shutting down TaskManager development services..."

# --- Stop API ---
if [ -f "$API_PID_FILE" ]; then
    API_PID=$(cat "$API_PID_FILE")
    if ps -p $API_PID > /dev/null; then
        print_color $YELLOW "Stopping API process (PID: $API_PID)..."
        kill $API_PID
    else
        print_color $YELLOW "API process (PID: $API_PID) not found."
    fi
    rm -f "$API_PID_FILE"
else
    print_color $YELLOW "API PID file not found. Maybe it was not running."
fi

# --- Stop Frontend ---
if [ -f "$FRONTEND_PID_FILE" ]; then
    FRONTEND_PID=$(cat "$FRONTEND_PID_FILE")
    if ps -p $FRONTEND_PID > /dev/null; then
        print_color $YELLOW "Stopping Frontend process (PID: $FRONTEND_PID)..."
        kill $FRONTEND_PID
    else
        print_color $YELLOW "Frontend process (PID: $FRONTEND_PID) not found."
    fi
    rm -f "$FRONTEND_PID_FILE"
else
    print_color $YELLOW "Frontend PID file not found. Maybe it was not running."
fi

# --- Forceful cleanup as a fallback ---
print_color $YELLOW "Running fallback cleanup for any orphaned processes..."
lsof -t -i:5001 | xargs kill -9 >/dev/null 2>&1 || true
lsof -t -i:5173 | xargs kill -9 >/dev/null 2>&1 || true

print_color $GREEN "Shutdown complete."