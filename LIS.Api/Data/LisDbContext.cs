using LIS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LIS.Api.Data;

public class LisDbContext : DbContext
{
    public LisDbContext(DbContextOptions<LisDbContext> options) : base(options)
    {
    }

    public DbSet<LabOrder> Orders => Set<LabOrder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LisDbContext).Assembly);
    }
}
