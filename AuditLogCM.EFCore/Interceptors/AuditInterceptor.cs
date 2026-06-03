using AuditLogCM.Core.Enums;
using AuditLogCM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using AuditLogCM.Core.Models;
using AuditLogCM.EFCore.DbContext;

namespace AuditLogCM.EFCore.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditSerializer _serializador;
        private readonly ICurrentUserResolver _currentUserResolver;
        private readonly AuditDbContext _context;

        public AuditInterceptor(
            IAuditSerializer serializador,
            ICurrentUserResolver currentUserResolver,
            AuditDbContext context)
        {
            _serializador = serializador;
            _currentUserResolver = currentUserResolver;
            _context = context;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context == null) return base.SavingChanges(eventData, result);

            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {

                    var propiedadeChave = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());

                    var valorChave = entry.State == EntityState.Deleted ? propiedadeChave?.OriginalValue : propiedadeChave?.CurrentValue;


                    string? valoresAnteriores = null;
                    string? valoresNovos = null;

                    if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                    {
                        var valoresAnterioresDict = entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString());
                        valoresAnteriores = _serializador.Serializar(valoresAnterioresDict);
                    }

                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        var valoresNovosDict = entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]);
                        valoresNovos = _serializador.Serializar(valoresNovosDict);
                    }

                    var auditEntry = new AuditEntry
                    {
                        IDAuditEntry = Guid.NewGuid(),
                        NomeTabelaAfetada = entry.Metadata.GetTableName() ?? string.Empty,
                        IDTabelaAfetada = valorChave?.ToString() ?? string.Empty,
                        Acao = entry.State switch
                        {
                            EntityState.Added => AuditAction.Create,
                            EntityState.Modified => AuditAction.Update,
                            EntityState.Deleted => AuditAction.Delete,
                            _ => throw new InvalidOperationException()
                        },
                        IDUsuario = _currentUserResolver.GetCurrentUserId(),
                        NomeUsuario = _currentUserResolver.GetCurrentUserName() ?? string.Empty,
                        ValoresAnteriores = valoresAnteriores,
                        ValoresNovos = valoresNovos
                    };
                    _context.AuditEntries.Add(auditEntry);
                }
            }
            try
            {
                _context.SaveChanges();
                return base.SavingChanges(eventData, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro de banco de dados detectado: {ex.Message}");
                throw;

            }

        }
    }
}
