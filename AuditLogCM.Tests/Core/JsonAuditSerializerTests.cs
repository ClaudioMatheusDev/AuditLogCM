using AuditLogCM.Core.Enums;
using AuditLogCM.Core.Models;
using AuditLogCM.EFCore.Serializers;
using FluentAssertions;

namespace AuditLogCM.Tests.Core
{
    public class JsonAuditSerializerTests
    {
        [Fact]
        public void DeveRetornarStringNaoVaziaAoSerializarObjeto()
        {
            var log = new AuditEntry
            {
                IDAuditEntry = Guid.NewGuid(),
                IDTabelaAfetada = "12",
                NomeTabelaAfetada = "TabelaUnitXtEST",
                ValoresAnteriores = "UM",
                ValoresNovos = "DOIS",
                Acao = AuditAction.Create,
                IDUsuario = "user123",
                NomeUsuario = "Claudio",
                DataAlteracao = DateTime.UtcNow,

            };

            var serializer = new JsonAuditSerializer();
            var json = serializer.Serializar(log);

            json.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void DeveRetornarStringVaziaAoSerializarObjetoNulo()
        {
            var serializer = new JsonAuditSerializer();
            var json = serializer.Serializar<AuditEntry>(null!);
            json.Should().BeEmpty();
        }

        [Fact]
        public void DeveTerConteudoAoSerializar()
        {
            var log = new AuditEntry
            {
                IDAuditEntry = Guid.NewGuid(),
                IDTabelaAfetada = "12",
                NomeTabelaAfetada = "TabelaUnitXtEST",
                ValoresAnteriores = "UM",
                ValoresNovos = "DOIS",
                Acao = AuditAction.Create,
                IDUsuario = "user123",
                NomeUsuario = "Claudio",
                DataAlteracao = DateTime.UtcNow,
            };
            var serializer = new JsonAuditSerializer();
            var json = serializer.Serializar(log);
            json.Should().Contain("IDAuditEntry");
            json.Should().Contain("IDTabelaAfetada");
            json.Should().Contain("NomeTabelaAfetada");
            json.Should().Contain("ValoresAnteriores");
            json.Should().Contain("ValoresNovos");
            json.Should().Contain("Acao");
            json.Should().Contain("IDUsuario");
            json.Should().Contain("NomeUsuario");
            json.Should().Contain("DataAlteracao");
        }
    }
}
