namespace Startup_Project.Application.Dtos.Payment;

public record SessionPaymentRequestDto(Guid ScheduledSessionId);

public class SessionPaymentResponseDto
{
    public string OrderId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public string BankSessionId { get; set; } = null!;
}

public class SessionPaymentStatusDto
{
    public Guid Id { get; set; }
    public string OrderId { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public string TrainerName { get; set; } = null!;
    public string SessionTitle { get; set; } = null!;
    public DateTime SessionDate { get; set; }
    public string SessionTime { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class MyPaymentsResponseDto
{
    public int TotalCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<SessionPaymentStatusDto> Payments { get; set; } = [];
}
