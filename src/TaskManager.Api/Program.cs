using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Services;
using TaskManager.Api.DTOs;

namespace TaskManager.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "TaskManager API",
                    Version = "1.1.0",
                    Description = "An efficient task management API"
                });
            });

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Add Entity Framework
            builder.Services.AddDbContext<TaskContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            // Health check endpoint
            app.MapGet("/health", () => new HealthCheckDto
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.1.0"
            });

            // API info endpoint
            app.MapGet("/api", () => new ApiInfoDto
            {
                Name = "TaskManager API",
                Version = "1.1.0",
                Description = "An efficient task management API",
                Endpoints = new ApiEndpointsDto
                {
                    Tasks = "/api/tasks",
                    Statistics = "/api/tasks/statistics",
                    Health = "/health"
                }
            });

            // Initialize database
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
                
                app.Logger.LogInformation("Creating database...");
                context.Database.EnsureCreated();
                app.Logger.LogInformation("Database created successfully.");
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred while initializing the database");
            }

            app.Run();
        }
    }
}
