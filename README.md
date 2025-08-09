# TaskManager - Vue.js + .NET Task Management App

A simple task management application with Vue 3 frontend and .NET 8 backend.

## Features

- Create, edit, and delete tasks
- Mark tasks as complete/incomplete
- Set task priority (Low, Medium, High)
- Add due dates with notifications
- Categorize tasks with colors
- Search and filter tasks
- Dark/light theme toggle
- Task statistics dashboard

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
   ./quick-start.sh
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
- `quick-start.sh` - Start everything
- `deploy-db.sh` - Database migrations
- `build.sh` - Production build

## API Endpoints

**Tasks:**
- `GET /api/tasks` - Get all tasks
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `PATCH /api/tasks/{id}/toggle` - Toggle completion
- `DELETE /api/tasks/{id}` - Delete task

**Categories:**
- `GET /api/categories` - Get all categories

**Statistics:**
- `GET /api/tasks/statistics` - Task counts

## Database Tables

**Tasks:**
- id, title, description, priority, category
- dueDate, estimatedHours, notificationsEnabled
- isCompleted, createdAt, updatedAt

**Categories:**
- id, name, color, displayOrder

## Development

**Start development:**
```bash
./quick-start.sh
```

**Database changes:**
```bash
./deploy-db.sh
```

**Build for production:**
```bash
./build.sh
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
- Priority levels with custom icons
- Due dates with browser notifications
- Category system with color coding

**User Interface:**
- Responsive design for all devices
- Dark/light theme support
- Real-time search and filtering
- Optimistic updates for better UX

**Data Management:**
- SQLite database with migrations
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
- Run `./deploy-db.sh` to reset database
- Check SQLite file permissions
- Verify migration status

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test everything works
5. Submit a pull request

## License

MIT License - feel free to use this project for learning or commercial purposes.

