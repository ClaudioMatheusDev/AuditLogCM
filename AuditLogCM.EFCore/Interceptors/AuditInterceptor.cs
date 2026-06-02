using AuditLogCM.Core.Enums;
using AuditLogCM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AuditLogCM.Core.Models;

namespace AuditLogCM.EFCore.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditSerializer _serializador;
        private readonly ICurrentUserResolver _currentUserResolver;

        public AuditInterceptor(
            IAuditSerializer serializador,
            ICurrentUserResolver currentUserResolver)
        {
            _serializador = serializador;
            _currentUserResolver = currentUserResolver;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context == null) return base.SavingChanges(eventData, result);

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added ||
                    entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified ||
                    entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
                {
                    var auditEntry = new AuditEntry
                    {
                        IDAuditEntry = Guid.NewGuid(),
                        NomeTabelaAfetada = entry.Metadata.GetTableName(),
                        Acao = entry.State switch
                        {
                            EntityState.Added => AuditAction.Create,
                            EntityState.Modified => AuditAction.Update,
                            EntityState.Deleted => AuditAction.Delete,
                            _ => throw new InvalidOperationException()
                        },
                        IDUsuario = _currentUserResolver.GetCurrentUserId(),
                        NomeUsuario = _currentUserResolver.GetCurrentUserName()
                    };
                }
            }

            return base.SavingChanges(eventData, result);
        }
    }
}
