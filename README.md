# TaskManager - Full Stack Task Management Application

A modern, enterprise-grade task management application built with **Vue 3 + TypeScript** frontend and **.NET 8** backend, featuring stored procedures and clean architecture.

## 🚀 Features

### Core Functionality
- ✅ **Create, Read, Update, Delete** tasks
- ✅ **Toggle task completion** status
- ✅ **Task prioritization** (Low, Medium, High)
- ✅ **Task categorization** with color coding
- ✅ **Advanced search and filtering**
- ✅ **Real-time task statistics**

### Enhanced Features
- 🔍 **Full-text search** across titles and descriptions
- 📊 **Task analytics** and completion metrics
- 🏷️ **Smart categorization** with visual indicators
- ⚡ **Optimistic updates** for better UX
- 📱 **Responsive design** for all devices
- 🎨 **Modern UI** with Tailwind CSS

### Technical Excellence
- 🏗️ **Clean Architecture** with separation of concerns
- 📦 **Stored Procedures** for optimal database performance
- 🔄 **RESTful API** with comprehensive error handling
- 🎯 **TypeScript** for type safety
- 🧪 **Enterprise patterns** (Repository, Unit of Work, AutoMapper)
- 📝 **Comprehensive documentation**

## 🛠️ Technology Stack

### Frontend
- **Vue 3** with Composition API
- **TypeScript** for type safety
- **Pinia** for state management
- **Axios** for HTTP client
- **Tailwind CSS** for styling
- **Vite** for build tooling

### Backend
- **.NET 8** Web API
- **Entity Framework Core** for data access
- **SQLite** database with stored procedures
- **AutoMapper** for object mapping
- **FluentValidation** for input validation
- **Swagger/OpenAPI** for documentation

### Database
- **SQLite** with optimized schema
- **Stored procedures** (implemented as views in SQLite)
- **Indexes** for performance optimization
- **Soft delete** support
- **Audit trails** with timestamps

## 📁 Project Structure

```
TaskManager/
├── src/
│   ├── TaskManager.Api/          # .NET 8 Web API
│   │   ├── Controllers/          # REST API endpoints
│   │   ├── Services/             # Business logic layer
│   │   ├── Models/               # Entity models
│   │   ├── DTOs/                 # Data transfer objects
│   │   ├── Data/                 # Database context
│   │   └── Program.cs            # Application entry point
│   └── TaskManager.Web/          # Vue 3 Frontend
│       ├── src/
│       │   ├── components/       # Vue components
│       │   ├── stores/           # Pinia stores
│       │   ├── services/         # API services
│       │   ├── types/            # TypeScript types
│       │   └── App.vue           # Root component
│       ├── package.json
│       └── vite.config.ts
├── database/
│   ├── schema/                   # Database schema scripts
│   └── stored_procedures/        # SQL procedures and views
├── scripts/
│   ├── run-dev.sh               # Development startup script
│   ├── build.sh                 # Production build script
│   └── setup-database.sh        # Database setup script
└── README.md
```

## 🚀 Quick Start

### Prerequisites
- **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download))
- **Node.js 18+** ([Download](https://nodejs.org/))
- **SQLite3** (usually pre-installed on macOS/Linux)

### Option 1: One-Command Setup (Recommended)
```bash
# Clone the repository
git clone https://github.com/your-repo/taskmanager.git
cd taskmanager

# Start everything in development mode
./scripts/run-dev.sh
```

This script will:
- ✅ Check all prerequisites
- ✅ Install dependencies
- ✅ Set up the database
- ✅ Start both API and frontend
- ✅ Open your browser automatically

### Option 2: Manual Setup

#### 1. Database Setup
```bash
# Create and populate the database
./scripts/setup-database.sh

# Or for production environment
./scripts/setup-database.sh --environment production
```

#### 2. Backend Setup
```bash
cd src/TaskManager.Api

# Restore packages
dotnet restore

# Run in development mode
dotnet run
```

#### 3. Frontend Setup
```bash
cd src/TaskManager.Web

# Install dependencies
npm install

# Start development server
npm run dev
```

## 🌐 Application URLs

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:5173 | Main application interface |
| **API** | https://localhost:7001 | REST API endpoints |
| **Swagger** | https://localhost:7001/swagger | API documentation |
| **Health** | https://localhost:7001/health | Health check endpoint |

## 📊 API Endpoints

### Tasks
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `PATCH /api/tasks/{id}/toggle` - Toggle completion
- `DELETE /api/tasks/{id}` - Delete task (soft delete)

### Advanced Operations
- `POST /api/tasks/search` - Advanced search with pagination
- `GET /api/tasks/statistics` - Task analytics
- `GET /api/tasks/priority/{priority}` - Filter by priority
- `PATCH /api/tasks/bulk` - Bulk update operations
- `DELETE /api/tasks/bulk` - Bulk delete operations

## 🗄️ Database Schema

### Tables
- **Tasks** - Main task storage with soft delete support
- **Categories** - Task categorization system

### Key Features
- **Optimized indexes** for fast queries
- **Audit trails** with created/updated timestamps
- **Soft delete** for data recovery
- **Check constraints** for data integrity
- **Foreign key relationships** with proper cascading

### Stored Procedures (SQLite Views)
- `sp_GetAllTasks` - Optimized task retrieval
- `sp_GetTaskStatistics` - Performance analytics
- `sp_SearchTasks` - Full-text search functionality

## 🏗️ Production Build

### Build for Production
```bash
# Create production build
./scripts/build.sh
```

This generates:
- Optimized .NET API binary
- Minified Vue.js assets
- Docker configuration
- Deployment scripts

### Deployment Options

#### Option 1: Direct Deployment
```bash
cd build
./start.sh  # Linux/macOS
# or
start.bat   # Windows
```

#### Option 2: Docker Deployment
```bash
cd build
docker-compose up -d
```

#### Option 3: Manual Deployment
```bash
cd build/api
dotnet TaskManager.Api.dll
```

## 🔧 Configuration

### Environment Variables
```bash
# API Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001
ConnectionStrings__DefaultConnection=Data Source=taskmanager.db

# Frontend Configuration
VITE_API_BASE_URL=https://localhost:7001/api
```

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=taskmanager.db"
  }
}
```

## 🧪 Development

### Code Structure
- **Clean Architecture** with clear separation of concerns
- **SOLID principles** implementation
- **Dependency injection** throughout
- **Repository pattern** for data access
- **AutoMapper** for DTO mapping

### Key Patterns Used
- **Unit of Work** for transaction management
- **Specification pattern** for query logic
- **CQRS** separation for read/write operations
- **Domain events** for loose coupling

### Error Handling
- **Global exception handling** in API
- **Validation** at multiple layers
- **Structured logging** with correlation IDs
- **User-friendly error messages**

## 🎯 Performance Optimizations

### Database
- **Indexed queries** for fast retrieval
- **Stored procedures** for complex operations
- **Connection pooling** for scalability
- **Optimistic concurrency** control

### Frontend
- **Vue 3 Composition API** for better performance
- **Lazy loading** of components
- **Optimistic updates** for responsive UI
- **Debounced search** to reduce API calls

### API
- **Async/await** throughout
- **Response caching** for static data
- **Pagination** for large datasets
- **Compression** for reduced payload sizes

## 🔒 Security Features

- **Input validation** on all endpoints
- **SQL injection protection** via parameterized queries
- **XSS prevention** with proper encoding
- **CORS configuration** for frontend integration
- **HTTPS enforcement** in production

## 📈 Monitoring & Observability

- **Health checks** for service monitoring
- **Structured logging** with Serilog
- **Performance counters** for metrics
- **Error tracking** and reporting

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Vue.js team** for the excellent framework
- **Microsoft** for .NET 8 and Entity Framework Core
- **SQLite** for the reliable database engine
- **Tailwind CSS** for the utility-first CSS framework

---

**Built with ❤️ using Vue 3 and .NET 8**

*This project demonstrates modern full-stack development practices with enterprise-grade patterns and clean architecture.*

