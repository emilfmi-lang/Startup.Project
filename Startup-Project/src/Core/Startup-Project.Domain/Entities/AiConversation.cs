using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Domain.Entities;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    public List<AiMessage> Messages { get; private set; } = [];

    private AiConversation() { }

    public AiConversation(Guid userId, string title)
    {
        UserId = userId;
        Title = title.Length > 100 ? title[..100] : title;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        Title = title.Length > 100 ? title[..100] : title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
