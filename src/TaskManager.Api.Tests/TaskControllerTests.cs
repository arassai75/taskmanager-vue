using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using Xunit;

namespace TaskManager.Api.Tests
{
    public class TaskControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TaskControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<TaskContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<TaskContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetTasks_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetTasks_ReturnsListOfTasks()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks");
            var content = await response.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<TaskDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(tasks);
            Assert.IsType<List<TaskDto>>(tasks);
        }

        [Fact]
        public async Task GetTaskById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateTask_WithValidData_ReturnsCreatedTask()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "New Test Task",
                Description = "New Test Description",
                Priority = 2, // Medium
                CategoryId = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(createTaskDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/tasks", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdTask = JsonSerializer.Deserialize<TaskDto>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(createdTask);
            Assert.Equal(createTaskDto.Title, createdTask.Title);
            Assert.Equal(createTaskDto.Description, createdTask.Description);
            Assert.Equal(createTaskDto.Priority, createdTask.Priority);
            Assert.False(createdTask.IsCompleted);
        }

        [Fact]
        public async Task CreateTask_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createTaskDto = new CreateTaskDto
            {
                Title = "", // Invalid: empty title
                Description = "Test Description",
                Priority = 2, // Medium
                CategoryId = 1
            };

            var content = new StringContent(
                JsonSerializer.Serialize(createTaskDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/tasks", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetStatistics_ReturnsValidStatistics()
        {
            // Act
            var response = await _client.GetAsync("/api/tasks/statistics");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var statistics = JsonSerializer.Deserialize<TaskStatisticsDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(statistics);
            Assert.IsType<int>(statistics.TotalTasks);
            Assert.IsType<int>(statistics.CompletedTasks);
            Assert.IsType<int>(statistics.PendingTasks);
            Assert.True(statistics.TotalTasks >= 0);
            Assert.True(statistics.CompletedTasks >= 0);
            Assert.True(statistics.PendingTasks >= 0);
            Assert.Equal(statistics.TotalTasks, statistics.CompletedTasks + statistics.PendingTasks);
        }

        [Fact]
        public async Task Health_Endpoint_ReturnsHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var health = JsonSerializer.Deserialize<HealthResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(health);
            Assert.Equal("Healthy", health.Status);
        }

        [Fact]
        public async Task Api_Info_Endpoint_ReturnsApiInfo()
        {
            // Act
            var response = await _client.GetAsync("/api");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var apiInfo = JsonSerializer.Deserialize<ApiInfoResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(apiInfo);
            Assert.Equal("TaskManager API", apiInfo.Name);
            Assert.Equal("1.1.0", apiInfo.Version);
        }
    }

    // Helper classes for deserialization
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
    }

    public class ApiInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object Endpoints { get; set; } = new();
    }
}
