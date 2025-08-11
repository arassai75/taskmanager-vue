# TaskManager - Vue.js + .NET Task Management App

A simple task management application with Vue 3 frontend and .NET 8 backend.

## Features

- Create, edit, and delete tasks
- Mark tasks as complete/incomplete
- Set task priority (Low, Medium, High)
- Categorize tasks with colors
- Search and filter tasks
- Task statistics dashboard
- Responsive design for all devices

## Tech Stack

**Frontend:**
- Vue 3 with TypeScript
- Pinia for state management
- Tailwind CSS for styling
- Vite for building

**Backend:**
- .NET 8 Web API
- Entity Framework Core
- SQLite database
- AutoMapper for data mapping

## Quick Start

1. **Clone the repo:**
   ```bash
   git clone https://github.com/your-repo/taskmanager.git
   cd taskmanager
   ```

2. **Run everything:**
   ```bash
   ./scripts/run-dev.sh
   ```

3. **Open in browser:**
   - Frontend: http://localhost:5173
   - API: https://localhost:7001

## Project Structure

**Backend (.NET API):**
- `src/TaskManager.Api/Controllers/` - API endpoints
- `src/TaskManager.Api/Services/` - Business logic
- `src/TaskManager.Api/Models/` - Database models
- `src/TaskManager.Api/Data/` - Database context

**Frontend (Vue.js):**
- `src/TaskManager.Web/src/components/` - Vue components
- `src/TaskManager.Web/src/stores/` - State management
- `src/TaskManager.Web/src/services/` - API calls
- `src/TaskManager.Web/src/types/` - TypeScript types

**Database:**
- `database/schema/` - SQL scripts
- `database/stored_procedures/` - Database views

**Scripts:**
- `run-dev.sh` - Start everything
- `build.sh` - Production build
- `setup-database.sh` - Database setup

## API Endpoints

**Tasks:**
- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `PATCH /api/tasks/{id}/toggle` - Toggle completion
- `DELETE /api/tasks/{id}` - Delete task

**Statistics:**
- `GET /api/tasks/statistics` - Task counts

## Database Tables

**Tasks:**
- id, title, description, priority, category
- isCompleted, createdAt, updatedAt

**Categories:**
- id, name, color, displayOrder

## Development

**Start development:**
```bash
./scripts/run-dev.sh
```

**Build for production:**
```bash
./scripts/build.sh
```
**Stop and Kill the current processes:**
```bash
pkill -f 'dotnet run' && pkill -f 'vite'
```
## Environment Variables

**API:**
- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=https://localhost:7001`
- `ConnectionStrings__DefaultConnection=Data Source=taskmanager.db`

**Frontend:**
- `VITE_API_BASE_URL=https://localhost:7001/api`

## Key Features Explained

**Task Management:**
- Full CRUD operations for tasks
- Priority levels with icons
- Category system with color coding
- Search and filtering capabilities

**User Interface:**
- Responsive design for all devices
- Real-time search and filtering
- Optimistic updates for better UX
- Modern UI with Tailwind CSS

**Data Management:**
- SQLite database with stored procedures
- Entity Framework Core ORM
- AutoMapper for data transformation
- Repository pattern for data access

## Troubleshooting

**API not starting:**
- Check if port 7001 is available
- Run `dotnet restore` in API folder
- Check database file permissions

**Frontend not loading:**
- Check if port 5173 is available
- Run `npm install` in Web folder
- Check API connection

**Database issues:**
- Run `./scripts/setup-database.sh` to reset database
- Check SQLite file permissions
- Verify database file exists


