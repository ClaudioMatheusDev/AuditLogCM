
namespace AuditLogCM.Core.Models
{
    public class AuditEntry
    {
        public Guid IDAuditEntry { get; set; }
        public string IDTabelaAfetada { get; set; } = string.Empty;
        public string NomeTabelaAfetada { get; set; } = string.Empty;
        public string? ValoresAnteriores { get; set; }
        public string? ValoresNovos { get; set; }
        public Enums.AuditAction Acao { get; set; }
        public string? IDUsuario { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;


    }
}
