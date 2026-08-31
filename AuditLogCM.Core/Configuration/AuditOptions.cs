namespace AuditLogCM.Core.Configuration
{
    /// <summary>
    /// Opções de configuração da auditoria: quais tipos de entidade devem ser ignorados.
    /// </summary>
    public class AuditOptions
    {
        public HashSet<Type> EntidadesIgnoradas { get; } = new();

        public bool DeveAuditar(Type tipoEntidade) => !EntidadesIgnoradas.Contains(tipoEntidade);
    }

    public static class AuditOptionsExtensions
    {
        /// <summary>
        /// Exclui um tipo de entidade da auditoria (nenhuma linha de auditoria será gerada para ele).
        /// </summary>
        public static AuditOptions Ignorar<TEntidade>(this AuditOptions options)
        {
            options.EntidadesIgnoradas.Add(typeof(TEntidade));
            return options;
        }
    }
}
