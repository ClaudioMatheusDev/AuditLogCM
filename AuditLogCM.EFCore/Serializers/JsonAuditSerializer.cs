using AuditLogCM.Core.Interfaces;
using System.Text.Json;

namespace AuditLogCM.EFCore.Serializers
{
    public class JsonAuditSerializer : IAuditSerializer
    {

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public string Serializar<T>(T objeto)
        {
            if (objeto == null) return string.Empty;

            return JsonSerializer.Serialize(objeto, _options);
        }


    }
}
