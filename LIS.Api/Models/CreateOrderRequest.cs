namespace LIS.Api.Models;

public class CreateOrderRequest
{
    public string PatientName { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateOnly CollectionDate { get; set; }
}
