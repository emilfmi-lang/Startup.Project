namespace Startup_Project.Application.Dtos.Account;

public class AddPaymentMethodDto
{
    public string CardNumber { get; set; } = null!;
    public string CardHolderName { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public bool IsDefault { get; set; }
}
