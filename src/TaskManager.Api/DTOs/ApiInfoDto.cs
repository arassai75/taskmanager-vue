namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO for API health check response
/// </summary>
public class HealthCheckDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// DTO for API endpoints information
/// </summary>
public class ApiEndpointsDto
{
    public string Tasks { get; set; } = string.Empty;
    public string Statistics { get; set; } = string.Empty;
    public string Health { get; set; } = string.Empty;
}

/// <summary>
/// DTO for API information response
/// </summary>
public class ApiInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ApiEndpointsDto Endpoints { get; set; } = new();
}
