using AutoMapper;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// Centralizes all mapping configurations for maintainability
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        ConfigureTaskMappings();
        ConfigureCategoryMappings();
    }

    /// <summary>
    /// Configures mappings for Task entities and DTOs
    /// </summary>
    private void ConfigureTaskMappings()
    {
        // TaskItem to TaskDto mapping
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => 
                src.Category != null ? src.Category.Name : "Uncategorized"))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => 
                src.Category != null ? src.Category.Color : null))
            .ForMember(dest => dest.PriorityText, opt => opt.MapFrom(src => src.PriorityText));

        // CreateTaskDto to TaskItem mapping
        CreateMap<CreateTaskDto, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .AfterMap((src, dest) => src.Normalize());

        // UpdateTaskDto to TaskItem mapping
        CreateMap<UpdateTaskDto, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .AfterMap((src, dest) => src.Normalize());
    }

    /// <summary>
    /// Configures mappings for Category entities and DTOs
    /// </summary>
    private void ConfigureCategoryMappings()
    {
        // Category to CategoryDto mapping
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ActiveTaskCount, opt => opt.MapFrom(src => src.ActiveTaskCount))
            .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.CompletedTaskCount))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => src.CompletionPercentage));

        // CreateCategoryDto to Category mapping
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .AfterMap((src, dest) => src.Normalize());

        // UpdateCategoryDto to Category mapping
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .AfterMap((src, dest) => src.Normalize());
    }
}

