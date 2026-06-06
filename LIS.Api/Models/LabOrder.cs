namespace LIS.Api.Models;

public class LabOrder
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string TestType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateOnly CollectionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
