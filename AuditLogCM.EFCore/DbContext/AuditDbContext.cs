using AuditLogCM.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditLogCM.EFCore.DbContext
{
    public class AuditDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<AuditEntry> AuditEntries { get; set; }

        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }
    }
}
