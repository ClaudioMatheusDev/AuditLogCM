namespace AuditLogCM.Core.Attributes
{
    /// <summary>
    /// Marca uma propriedade para que seu valor nunca seja incluído nos registros de auditoria
    /// (útil para senhas, hashes, tokens e outros dados sensíveis).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AuditIgnoreAttribute : Attribute
    {
    }
}
