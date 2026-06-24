namespace LIS.Api.Models;

public class LabOrder
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public TestType TestType { get; set; }
    public Priority Priority { get; set; }
    public DateOnly CollectionDate { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Id of the authenticated user who created the order (audit linkage).</summary>
    public Guid? CreatedByUserId { get; set; }
}
