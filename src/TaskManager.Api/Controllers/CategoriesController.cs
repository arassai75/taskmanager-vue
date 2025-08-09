using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

/// <summary>
/// REST API controller for category management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active categories
    /// </summary>
    /// <returns>List of active categories</returns>
    /// <response code="200">Returns the list of categories</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
        try
        {
            _logger.LogInformation("GET /api/categories called");
            var categories = await _categoryService.GetAllCategoriesAsync();
            _logger.LogInformation("Retrieved {Count} categories", categories.Count());
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return Problem(
                title: "Error retrieving categories",
                detail: "An error occurred while retrieving categories. Please try again later.",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// Retrieves a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    /// <response code="200">Returns the category</response>
    /// <response code="404">Category not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        try
        {
            _logger.LogInformation("GET /api/categories/{Id} called", id);
            var category = await _categoryService.GetCategoryByIdAsync(id);
            
            if (category == null)
            {
                _logger.LogWarning("Category with ID {Id} not found", id);
                return NotFound(new ProblemDetails
                {
                    Title = "Category not found",
                    Detail = $"Category with ID {id} was not found.",
                    Status = 404
                });
            }

            _logger.LogInformation("Retrieved category: {CategoryName}", category.Name);
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {Id}", id);
            return Problem(
                title: "Error retrieving category",
                detail: "An error occurred while retrieving the category. Please try again later.",
                statusCode: 500
            );
        }
    }
}
