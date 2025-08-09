using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

/// <summary>
/// Entity Framework DbContext for the TaskManager application
/// Handles database operations and entity configuration
/// </summary>
public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for Task entities
    /// </summary>
    public DbSet<TaskItem> Tasks { get; set; }

    /// <summary>
    /// DbSet for Category entities
    /// </summary>
    public DbSet<Category> Categories { get; set; }



    /// <summary>
    /// Configures entity relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TaskItem entity
        ConfigureTaskItem(modelBuilder);

        // Configure Category entity
        ConfigureCategory(modelBuilder);


        // Seed initial data
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Configures the TaskItem entity
    /// </summary>
    private static void ConfigureTaskItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            // Table configuration
            entity.ToTable("Tasks");
            
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Property configurations
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(1);
            
            entity.Property(e => e.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            // New v1.1.0 properties
            entity.Property(e => e.DueDate)
                .HasColumnType("DATETIME");
            
            entity.Property(e => e.EstimatedHours)
                .HasColumnType("DECIMAL(6,2)");
            
            entity.Property(e => e.NotificationsEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Tasks)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            entity.HasIndex(e => new { e.IsDeleted, e.CreatedAt })
                .HasDatabaseName("IX_Tasks_IsDeleted_CreatedAt");
            
            entity.HasIndex(e => new { e.IsCompleted, e.IsDeleted })
                .HasDatabaseName("IX_Tasks_IsCompleted_IsDeleted");
            
            entity.HasIndex(e => e.Title)
                .HasDatabaseName("IX_Tasks_Title");
            
            entity.HasIndex(e => new { e.IsCompleted, e.Priority, e.IsDeleted })
                .HasDatabaseName("IX_Tasks_Status_Priority");
            
            // New v1.1.0 indexes
            entity.HasIndex(e => e.DueDate)
                .HasDatabaseName("IX_Tasks_DueDate");
            
            entity.HasIndex(e => e.EstimatedHours)
                .HasDatabaseName("IX_Tasks_EstimatedHours");
            
            entity.HasIndex(e => new { e.DueDate, e.NotificationsEnabled, e.IsCompleted })
                .HasDatabaseName("IX_Tasks_Notifications");

            // Check constraints using modern EF Core syntax
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Tasks_Title_NotEmpty", "LENGTH(TRIM(Title)) > 0");
                t.HasCheckConstraint("CK_Tasks_Priority_Valid", "Priority IN (1, 2, 3)");
                t.HasCheckConstraint("CK_Tasks_DeletedAt_Logic", 
                    "(IsDeleted = 0 AND DeletedAt IS NULL) OR (IsDeleted = 1 AND DeletedAt IS NOT NULL)");
                t.HasCheckConstraint("CK_Tasks_EstimatedHours_Positive", 
                    "EstimatedHours IS NULL OR EstimatedHours > 0");
                t.HasCheckConstraint("CK_Tasks_EstimatedHours_Reasonable", 
                    "EstimatedHours IS NULL OR EstimatedHours <= 999.99");
            });
        });
    }

    /// <summary>
    /// Configures the Category entity
    /// </summary>
    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            // Table configuration
            entity.ToTable("Categories");
            
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Property configurations
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.Color)
                .HasMaxLength(7);
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Unique constraint on name
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name_Unique");

            // Check constraints using modern EF Core syntax
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Categories_Name_NotEmpty", "LENGTH(TRIM(Name)) > 0");
                t.HasCheckConstraint("CK_Categories_Color_Format", 
                    "Color IS NULL OR (LENGTH(Color) = 7 AND Color LIKE '#%')");
            });
        });
    }



    /// <summary>
    /// Seeds initial data for development and testing
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category 
            { 
                Id = 1, 
                Name = "General", 
                Description = "General tasks without specific category", 
                Color = "#6B7280",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category 
            { 
                Id = 2, 
                Name = "Work", 
                Description = "Work-related tasks and projects", 
                Color = "#3B82F6",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category 
            { 
                Id = 3, 
                Name = "Personal", 
                Description = "Personal tasks and reminders", 
                Color = "#10B981",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Category 
            { 
                Id = 4, 
                Name = "Urgent", 
                Description = "High priority urgent tasks", 
                Color = "#EF4444",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Sample Tasks
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem 
            { 
                Id = 1, 
                Title = "Complete project documentation", 
                Description = "Write comprehensive documentation for the TaskManager API",
                Priority = 2,
                CategoryId = 2,
                DueDate = now.AddDays(3),
                EstimatedHours = 2.5m,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            },
            new TaskItem 
            { 
                Id = 2, 
                Title = "Review pull requests", 
                Description = "Review and approve pending pull requests in the repository",
                Priority = 2,
                CategoryId = 2,
                DueDate = now.AddDays(1),
                EstimatedHours = 1.0m,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-3)
            },
            new TaskItem 
            { 
                Id = 3, 
                Title = "Buy groceries", 
                Description = "Weekly grocery shopping - milk, bread, fruits",
                Priority = 1,
                CategoryId = 3,
                DueDate = now.AddDays(7),
                EstimatedHours = 0.5m,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2)
            },
            new TaskItem 
            { 
                Id = 4, 
                Title = "Schedule dentist appointment", 
                Description = "Annual dental checkup appointment",
                Priority = 2,
                CategoryId = 3,
                DueDate = now.AddDays(2),
                EstimatedHours = 1.5m,
                NotificationsEnabled = true,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1)
            },
            new TaskItem 
            { 
                Id = 5, 
                Title = "Deploy to production", 
                Description = "Deploy the latest version to production environment",
                Priority = 3,
                CategoryId = 2,
                IsCompleted = true,
                EstimatedHours = 4.0m,
                NotificationsEnabled = false,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-1)
            }
        );



    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp for modified entities
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Modified && 
                       e.Entity is TaskItem);

        foreach (var entry in entries)
        {
            if (entry.Entity is TaskItem task)
            {
                task.UpdatedAt = DateTime.UtcNow;
                
                // Handle soft delete timestamp
                if (task.IsDeleted && task.DeletedAt == null)
                {
                    task.DeletedAt = DateTime.UtcNow;
                }
                else if (!task.IsDeleted && task.DeletedAt != null)
                {
                    task.DeletedAt = null;
                }
            }

        }
    }
}
