namespace Startup_Project.Application.Dtos.Account;

public class ActiveSessionDto
{
    public Guid Id { get; set; }
    public string DeviceInfo { get; set; } = null!;    
    public string? Location { get; set; }              
    public bool IsCurrent { get; set; }
    public string Status { get; set; } = null!;        
    public DateTime LastActiveAt { get; set; }
    public string LastActiveLabel { get; set; } = null!;
}
