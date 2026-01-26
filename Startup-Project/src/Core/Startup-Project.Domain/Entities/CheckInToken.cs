using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Startup_Project.Domain.Entities;

public class CheckInToken:BaseEntity
{
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public AppUser User { get; set; }

    public DateTime ExpiryDate { get; set; } 
    public bool IsUsed { get; set; }
}
