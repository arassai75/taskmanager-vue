#!/bin/bash

echo "🚀 Quick Start - TaskManager"
echo "============================="

# Kill any existing processes
pkill -f "dotnet run" >/dev/null 2>&1
pkill -f "vite" >/dev/null 2>&1
sleep 2

echo "📦 Starting API..."
cd src/TaskManager.Api

# Start API in background
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="http://localhost:5001" \
DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
DOTNET_CLI_TELEMETRY_OPTOUT=1 \
nohup dotnet run > api.log 2>&1 &

API_PID=$!
echo "✅ API started (PID: $API_PID)"

# Wait for API
echo "⏳ Waiting for API..."
for i in {1..15}; do
    if curl -s http://localhost:5001/health >/dev/null 2>&1; then
        echo "✅ API is ready!"
        break
    fi
    sleep 1
    printf "."
done
echo

# Start Frontend
echo "📦 Starting Frontend..."
cd ../TaskManager.Web

# Set API URL and start frontend
VITE_API_BASE_URL="http://localhost:5001/api" \
nohup npm run dev > frontend.log 2>&1 &

FRONTEND_PID=$!
echo "✅ Frontend started (PID: $FRONTEND_PID)"

echo ""
echo "🎉 TaskManager is running!"
echo "=========================="
echo "Frontend: http://localhost:5173"
echo "API:      http://localhost:5001"
echo "Swagger:  http://localhost:5001/swagger"
echo "Health:   http://localhost:5001/health"
echo ""
echo "📝 Logs:"
echo "API:      src/TaskManager.Api/api.log"
echo "Frontend: src/TaskManager.Web/frontend.log"
echo ""
echo "🛑 To stop: pkill -f 'dotnet run' && pkill -f 'vite'"
