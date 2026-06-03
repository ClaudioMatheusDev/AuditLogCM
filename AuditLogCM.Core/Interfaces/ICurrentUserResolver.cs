namespace AuditLogCM.Core.Interfaces
{
    public interface ICurrentUserResolver
    {
        public string? GetCurrentUserId();
        public string? GetCurrentUserName();


    }
}
