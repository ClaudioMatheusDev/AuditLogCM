namespace AuditLogCM.Core.Interfaces
{
	public interface IAuditSerializer
    {
        public string Serializar<T>(T objetoParaSerializar) ;
    }
}
