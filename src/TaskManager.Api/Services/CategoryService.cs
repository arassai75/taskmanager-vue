using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementation of category management services
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly TaskContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(TaskContext context, IMapper mapper, ILogger<CategoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active categories ordered by name
    /// </summary>
    /// <returns>Collection of active categories</returns>
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all active categories");
            
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            _logger.LogInformation("Retrieved {Count} active categories", categories.Count);
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category if found, null otherwise</returns>
    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching category with ID: {Id}", id);
            
            var category = await _context.Categories
                .Where(c => c.Id == id && c.IsActive)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found or inactive", id);
                return null;
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);
            
            _logger.LogInformation("Retrieved category: {CategoryName}", category.Name);
            return categoryDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category with ID {Id}", id);
            throw;
        }
    }
}
