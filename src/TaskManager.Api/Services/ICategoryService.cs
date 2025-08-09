using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Interface for category management services
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Retrieves all active categories
    /// </summary>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Retrieves a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
}
