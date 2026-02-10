namespace Startup_Project.Application.Dtos.Account;

public class PaymentMethodsResponseDto
{
    public List<PaymentMethodDto> Methods { get; set; } = [];
    public int TotalCount { get; set; }
}
