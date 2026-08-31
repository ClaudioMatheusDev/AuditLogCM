using System.Reflection;
using AuditLogCM.Core.Attributes;
using AuditLogCM.Core.Configuration;
using AuditLogCM.Core.Enums;
using AuditLogCM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using AuditLogCM.Core.Models;
using AuditLogCM.EFCore.Persistence;

namespace AuditLogCM.EFCore.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditSerializer _serializador;
        private readonly ICurrentUserResolver _currentUserResolver;
        private readonly AuditDbContext _context;
        private readonly AuditOptions _opcoes;
        private readonly ILogger<AuditInterceptor> _logger;

        private List<AuditEntry> _entradasPendentes = new();

        public AuditInterceptor(
            IAuditSerializer serializador,
            ICurrentUserResolver currentUserResolver,
            AuditDbContext context,
            AuditOptions? opcoes = null,
            ILogger<AuditInterceptor>? logger = null)
        {
            _serializador = serializador;
            _currentUserResolver = currentUserResolver;
            _context = context;
            _opcoes = opcoes ?? new AuditOptions();
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AuditInterceptor>.Instance;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            CapturarEntradas(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CapturarEntradas(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (_entradasPendentes.Count > 0)
            {
                try
                {
                    _context.AuditEntries.AddRange(_entradasPendentes);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gravar registros de auditoria.");
                    throw;
                }
                finally
                {
                    _entradasPendentes = new();
                }
            }

            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (_entradasPendentes.Count > 0)
            {
                try
                {
                    _context.AuditEntries.AddRange(_entradasPendentes);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gravar registros de auditoria.");
                    throw;
                }
                finally
                {
                    _entradasPendentes = new();
                }
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            _entradasPendentes = new();
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            _entradasPendentes = new();
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        private void CapturarEntradas(Microsoft.EntityFrameworkCore.DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (!_opcoes.DeveAuditar(entry.Entity.GetType())) continue;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var auditEntry = ConstruirAuditEntry(entry);
                    _entradasPendentes.Add(auditEntry);
                }
            }
        }

        private static bool DeveIgnorarPropriedade(IProperty propriedade) =>
            propriedade.PropertyInfo?.GetCustomAttribute<AuditIgnoreAttribute>() != null;

        private AuditEntry ConstruirAuditEntry(EntityEntry entry)
        {
            var propriedadesChave = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();

            var valorChave = string.Join(",", propriedadesChave.Select(p =>
                (entry.State == EntityState.Deleted ? p.OriginalValue : p.CurrentValue)?.ToString() ?? string.Empty));

            string? valoresAnteriores = null;
            string? valoresNovos = null;

            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                var valoresAnterioresDict = entry.OriginalValues.Properties
                    .Where(p => !DeveIgnorarPropriedade(p))
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString());
                valoresAnteriores = _serializador.Serializar(valoresAnterioresDict);
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                var valoresNovosDict = entry.CurrentValues.Properties
                    .Where(p => !DeveIgnorarPropriedade(p))
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]);
                valoresNovos = _serializador.Serializar(valoresNovosDict);
            }

            return new AuditEntry
            {
                IDAuditEntry = Guid.NewGuid(),
                NomeTabelaAfetada = entry.Metadata.GetTableName() ?? string.Empty,
                IDTabelaAfetada = valorChave,
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
        }
    }
}
