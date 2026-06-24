namespace LIS.Api.Models;

/// <summary>Lab test types accepted by the system. Stored as string in the database.</summary>
public enum TestType
{
    CBC,
    BMP,
    LipidPanel,
    UA
}

/// <summary>Order priority. STAT orders are surfaced/highlighted ahead of routine work.</summary>
public enum Priority
{
    Routine,
    STAT
}

/// <summary>Application roles used for authorization.</summary>
public enum UserRole
{
    Admin,
    Technician
}
