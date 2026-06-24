namespace LIS.Api.Models;

/// <summary>
/// Immutable audit record. Captures who did what, to which entity, and when —
/// required for a system handling patient lab data.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime TimestampUtc { get; set; }

    /// <summary>Optional JSON snippet with extra context (e.g. order summary).</summary>
    public string? Details { get; set; }
}
