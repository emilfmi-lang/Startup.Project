namespace Startup_Project.Application.Dtos.Account;

public class UserPreferencesDto
{
    public string Language { get; set; } = null!;     
    public string Theme { get; set; } = null!;        
    public string DistanceUnit { get; set; } = null!; 
    public string TimeZone { get; set; } = null!;     
}
