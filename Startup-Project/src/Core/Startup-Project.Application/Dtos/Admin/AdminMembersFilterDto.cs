using Startup_Project.Application.Dtos.Common;

namespace Startup_Project.Application.Dtos.Admin;

/// <summary>
/// Admin members query parameters
/// Admin multiple gyms own ed? bil?c?yi üçün gymId optional-d?r
/// </summary>
public class AdminMembersFilterDto
{
    /// <summary>
    /// Seçilmi? gym ID-si (optional, null olarsa bütün admin-in gym-l?rin? bax)
    /// </summary>
    public Guid? SelectedGymId { get; set; }

    /// <summary>
    /// S?hif? nömr?si
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// S?hif? ölçüsü
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Ad/email axtar???
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// V?ziyy?t filteri (active, inactive, suspended)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Plan filteri
    /// </summary>
    public string? Plan { get; set; }

    /// <summary>
    /// S?ralama sah?si (name, joinDate, expiresDate)
    /// </summary>
    public string? SortBy { get; set; } = "name";

    /// <summary>
    /// S?ralama qaydas? (asc, desc)
    /// </summary>
    public string? SortOrder { get; set; } = "asc";
}
