using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=taskmanager.db"));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register application services
builder.Services.AddScoped<ITaskService, TaskService>();

// Add CORS for Vue.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Vue dev server ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskManager API",
        Version = "v1",
        Description = "A comprehensive task management API with stored procedure support",
        Contact = new OpenApiContact
        {
            Name = "TaskManager Development Team",
            Email = "arassai75+taskmanager@gmail.com"
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add problem details for better error responses
builder.Services.AddProblemDetails();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "TaskManager API Documentation";
    });

    // Enable detailed error pages in development
    app.UseDeveloperExceptionPage();
}
else
{
    // Production error handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Enable CORS
app.UseCors("VueFrontend");

// Security headers
app.UseHttpsRedirection();

// API routing
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"
}));

// API info endpoint
app.MapGet("/api", () => Results.Ok(new
{
    Name = "TaskManager API",
    Version = "1.0.0",
    Description = "A comprehensive task management API with stored procedure support",
    Endpoints = new
    {
        Tasks = "/api/tasks",
        Documentation = "/swagger",
        Health = "/health"
    }
}));

// Error handling endpoint for production
if (!app.Environment.IsDevelopment())
{
    app.MapGet("/error", () => Results.Problem("An error occurred processing your request."));
}

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Ensuring database is created...");
        context.Database.EnsureCreated();
        
        // Optionally run migrations if they exist
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Running pending migrations...");
            context.Database.Migrate();
        }
        
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

// Log startup information
app.Logger.LogInformation("TaskManager API starting up...");
app.Logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Swagger UI available at: {SwaggerUrl}", "/swagger");

app.Run();
