namespace Startup_Project.Application.Dtos.Account;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string CardType { get; set; } = null!;
    public string CardHolderName { get; set; } = null!;
    public string Last4Digits { get; set; } = null!;
    public string MaskedNumber { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string ExpiryLabel { get; set; } = null!;
    public bool IsDefault { get; set; }
    public bool IsExpired { get; set; }
}
