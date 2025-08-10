using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO for returning category data to clients
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ActiveTaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public double CompletionPercentage { get; set; }
}

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF5733)")]
    public string? Color { get; set; }

    /// <summary>
    /// Validates and trims the input data
    /// </summary>
    public void Normalize()
    {
        Name = Name?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim().ToUpperInvariant();
    }
}

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF5733)")]
    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Validates and trims the input data
    /// </summary>
    public void Normalize()
    {
        Name = Name?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
        Color = string.IsNullOrWhiteSpace(Color) ? null : Color.Trim().ToUpperInvariant();
    }
}

