namespace LIS.Api.Models;

public record AuditLogResponse(
    long Id,
    Guid? UserId,
    string? Username,
    string Action,
    string? EntityType,
    string? EntityId,
    DateTime TimestampUtc,
    string? Details);
