using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using Xunit;

namespace TaskManager.Api.Tests
{
    public class TaskServiceTests : IDisposable
    {
        private readonly TaskContext _context;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            // Setup in-memory database for each test
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TaskContext(options);
            
            // Create a mock mapper and logger
            var mapper = new AutoMapper.Mapper(new AutoMapper.MapperConfiguration(cfg => 
            {
                cfg.CreateMap<TaskItem, TaskDto>();
                cfg.CreateMap<CreateTaskDto, TaskItem>();
                cfg.CreateMap<UpdateTaskDto, TaskItem>();
            }));
            
            // Create a simple mock logger
            var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<TaskService>();
            
            _taskService = new TaskService(_context, mapper, logger);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Work", Color = "#FF0000" },
                new Category { Id = 2, Name = "Personal", Color = "#00FF00" },
                new Category { Id = 3, Name = "Urgent", Color = "#FF0000" }
            };

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = 1,
                    Title = "Test Task 1",
                    Description = "Description 1",
                    Priority = 3, // High
                    CategoryId = 1,
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new TaskItem
                {
                    Id = 2,
                    Title = "Test Task 2",
                    Description = "Description 2",
                    Priority = 2, // Medium
                    CategoryId = 2,
                    IsCompleted = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Categories.AddRange(categories);
            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllTasksAsync_ReturnsAllTasks()
        {
            // Act
            var tasks = await _taskService.GetAllTasksAsync();

            // Assert
            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count());
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithValidId_ReturnsTask()
        {
            // Act
            var task = await _taskService.GetTaskByIdAsync(1);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(1, task.Id);
            Assert.Equal("Test Task 1", task.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var task = await _taskService.GetTaskByIdAsync(999);

            // Assert
            Assert.Null(task);
        }

        [Fact]
        public async Task CreateTaskAsync_WithValidData_CreatesTask()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description",
                Priority = 1, // Low
                CategoryId = 1
            };

            // Act
            var createdTask = await _taskService.CreateTaskAsync(createTaskDto);

            // Assert
            Assert.NotNull(createdTask);
            Assert.Equal(createTaskDto.Title, createdTask.Title);
            Assert.Equal(createTaskDto.Description, createdTask.Description);
            Assert.Equal(createTaskDto.Priority, createdTask.Priority);
            Assert.Equal(createTaskDto.CategoryId, createdTask.CategoryId);
            Assert.False(createdTask.IsCompleted);
            Assert.True(createdTask.Id > 0);

            // Verify it's saved in database
            var savedTask = await _context.Tasks.FindAsync(createdTask.Id);
            Assert.NotNull(savedTask);
            Assert.Equal(createTaskDto.Title, savedTask.Title);
        }

        [Fact]
        public async Task CreateTaskAsync_WithInvalidCategoryId_ThrowsException()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Task",
                Description = "New Description",
                Priority = 1, // Low
                CategoryId = 999 // Invalid category ID
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _taskService.CreateTaskAsync(createTaskDto));
        }

        [Fact]
        public async Task UpdateTaskAsync_WithValidData_UpdatesTask()
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = 3, // High
                CategoryId = 2
            };

            // Act
            var updatedTask = await _taskService.UpdateTaskAsync(1, updateTaskDto);

            // Assert
            Assert.NotNull(updatedTask);
            Assert.Equal(updateTaskDto.Title, updatedTask.Title);
            Assert.Equal(updateTaskDto.Description, updatedTask.Description);
            Assert.Equal(updateTaskDto.Priority, updatedTask.Priority);
            Assert.Equal(updateTaskDto.CategoryId, updatedTask.CategoryId);

            // Verify it's updated in database
            var savedTask = await _context.Tasks.FindAsync(1);
            Assert.NotNull(savedTask);
            Assert.Equal(updateTaskDto.Title, savedTask.Title);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var updateTaskDto = new UpdateTaskDto
            {
                Title = "Updated Task",
                Description = "Updated Description",
                Priority = 3, // High
                CategoryId = 1
            };

            // Act
            var updatedTask = await _taskService.UpdateTaskAsync(999, updateTaskDto);

            // Assert
            Assert.Null(updatedTask);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithValidId_DeletesTask()
        {
            // Act
            var result = await _taskService.DeleteTaskAsync(1);

            // Assert
            Assert.True(result);

            // Verify it's deleted from database
            var deletedTask = await _context.Tasks.FindAsync(1);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task DeleteTaskAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _taskService.DeleteTaskAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ToggleTaskCompletionAsync_WithValidId_TogglesCompletion()
        {
            // Arrange - Get initial state
            var initialTask = await _context.Tasks.FindAsync(1);
            var initialCompletionState = initialTask!.IsCompleted;

            // Act
            var toggledTask = await _taskService.ToggleTaskCompletionAsync(1);

            // Assert
            Assert.NotNull(toggledTask);
            Assert.Equal(!initialCompletionState, toggledTask.IsCompleted);

            // Verify it's updated in database
            var savedTask = await _context.Tasks.FindAsync(1);
            Assert.Equal(!initialCompletionState, savedTask!.IsCompleted);
        }

        [Fact]
        public async Task ToggleTaskCompletionAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var toggledTask = await _taskService.ToggleTaskCompletionAsync(999);

            // Assert
            Assert.Null(toggledTask);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
