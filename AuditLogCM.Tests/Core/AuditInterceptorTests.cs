using AuditLogCM.Core.Attributes;
using AuditLogCM.Core.Configuration;
using AuditLogCM.Core.Interfaces;
using AuditLogCM.EFCore.Persistence;
using AuditLogCM.EFCore.Interceptors;
using AuditLogCM.EFCore.Serializers;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AuditLogCM.Tests.Core
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        [AuditIgnore]
        public string Senha { get; set; } = string.Empty;
    }

    [PrimaryKey(nameof(PedidoId), nameof(ProdutoId))]
    public class PedidoItem
    {
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<PedidoItem> PedidoItens { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }

    public class AuditInterceptorTests
    {   
        [Fact]
        public void DeveRegistrarAuditoria_QuandoProdutoForAdicionado()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

        var mockResolver = new Mock<ICurrentUserResolver>();

        mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
        mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

        var resolver = mockResolver.Object;

        var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("app-test")
            .AddInterceptors(interceptor)
            .Options;

        using var appDbContext = new AppDbContext(appOptions);
        appDbContext.Produtos.Add(new Produto {Nome = "Produto Teste" });
        appDbContext.SaveChanges();

        auditDbContext.AuditEntries.Count().Should().Be(1);
    }
        [Fact]
    public void DeveRegistrarAuditoria_QuandoProdutoForAtualizado()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase("audit-test-update")
            .Options;

        using var auditDbContext = new AuditDbContext(options);

        var mockResolver = new Mock<ICurrentUserResolver>();

        mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
        mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

        var resolver = mockResolver.Object;

        var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("app-test-update")
            .AddInterceptors(interceptor)
            .Options;

        using var appDbContext = new AppDbContext(appOptions);
        var produto = new Produto { Nome = "Produto Teste" };
        appDbContext.Produtos.Add(produto);
        appDbContext.SaveChanges();

        produto.Nome = "Produto Atualizado";
        appDbContext.SaveChanges();

        auditDbContext.AuditEntries.Count().Should().Be(2);

        }

        [Fact]
        public void DeveRegistrarAuditoria_QuandoProdutoForRemovido()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-delete")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("app-test-delete")
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            var produto = new Produto { Nome = "Produto Teste" };
            appDbContext.Produtos.Add(produto);
            appDbContext.SaveChanges();

            appDbContext.Produtos.Remove(produto);
            appDbContext.SaveChanges();

            auditDbContext.AuditEntries.Count().Should().Be(2);
        }

        [Fact]
        public async Task DeveRegistrarAuditoria_QuandoProdutoForAdicionadoViaSaveChangesAsync()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-async")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("app-test-async")
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            appDbContext.Produtos.Add(new Produto { Nome = "Produto Teste" });
            await appDbContext.SaveChangesAsync();

            auditDbContext.AuditEntries.Count().Should().Be(1);
        }

        [Fact]
        public async Task NaoDeveRegistrarAuditoria_QuandoSaveChangesPrincipalFalhar()
        {
            const string dbName = "app-test-failure";

            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-failure")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .AddInterceptors(interceptor)
                .Options;

            using (var appDbContext = new AppDbContext(appOptions))
            {
                appDbContext.Produtos.Add(new Produto { Id = 1, Nome = "Produto Original" });
                await appDbContext.SaveChangesAsync();
            }

            auditDbContext.AuditEntries.Count().Should().Be(1);

            using (var appDbContext2 = new AppDbContext(appOptions))
            {
                appDbContext2.Produtos.Add(new Produto { Id = 1, Nome = "Produto Duplicado" });

                var act = async () => await appDbContext2.SaveChangesAsync();
                await act.Should().ThrowAsync<ArgumentException>();
            }

            auditDbContext.AuditEntries.Count().Should().Be(1);
        }

        [Fact]
        public void NaoDeveIncluirPropriedadeMarcadaComAuditIgnore()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-ignore-property")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("app-test-ignore-property")
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            appDbContext.Clientes.Add(new Cliente { Nome = "Ana", Senha = "segredo123" });
            appDbContext.SaveChanges();

            var entrada = auditDbContext.AuditEntries.Single();
            entrada.ValoresNovos.Should().Contain("Ana");
            entrada.ValoresNovos.Should().NotContain("segredo123");
        }

        [Fact]
        public void NaoDeveRegistrarAuditoria_QuandoEntidadeForIgnoradaViaOptions()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-ignore-entity")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var opcoes = new AuditOptions().Ignorar<Produto>();

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext, opcoes);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("app-test-ignore-entity")
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            appDbContext.Produtos.Add(new Produto { Nome = "Produto Teste" });
            appDbContext.SaveChanges();

            auditDbContext.AuditEntries.Count().Should().Be(0);
        }

        [Fact]
        public void DeveRegistrarChaveCompostaCorretamente()
        {
            var options = new DbContextOptionsBuilder<AuditDbContext>()
                .UseInMemoryDatabase("audit-test-composite-key")
                .Options;

            using var auditDbContext = new AuditDbContext(options);

            var mockResolver = new Mock<ICurrentUserResolver>();

            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var resolver = mockResolver.Object;

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), resolver, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("app-test-composite-key")
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            appDbContext.PedidoItens.Add(new PedidoItem { PedidoId = 10, ProdutoId = 20, Quantidade = 3 });
            appDbContext.SaveChanges();

            var entrada = auditDbContext.AuditEntries.Single();
            entrada.IDTabelaAfetada.Should().Be("10,20");
        }

        [Fact]
        public void DeveRegistrarIdRealGeradoPeloBanco_QuandoChavePrimariaForAutoincrementada()
        {
            // EF InMemory não usa chaves temporárias como os providers relacionais reais (SQL Server,
            // SQLite, PostgreSQL) usam antes do INSERT — por isso esse cenário exige SQLite de verdade.
            using var appConnection = new SqliteConnection("Data Source=:memory:");
            appConnection.Open();

            using var auditConnection = new SqliteConnection("Data Source=:memory:");
            auditConnection.Open();

            var auditOptions = new DbContextOptionsBuilder<AuditDbContext>()
                .UseSqlite(auditConnection)
                .Options;

            using var auditDbContext = new AuditDbContext(auditOptions);
            auditDbContext.Database.EnsureCreated();

            var mockResolver = new Mock<ICurrentUserResolver>();
            mockResolver.Setup(x => x.GetCurrentUserId()).Returns("test_user");
            mockResolver.Setup(x => x.GetCurrentUserName()).Returns("Test User");

            var interceptor = new AuditInterceptor(new JsonAuditSerializer(), mockResolver.Object, auditDbContext);

            var appOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(appConnection)
                .AddInterceptors(interceptor)
                .Options;

            using var appDbContext = new AppDbContext(appOptions);
            appDbContext.Database.EnsureCreated();

            var produto = new Produto { Nome = "Produto Autoincrementado" };
            appDbContext.Produtos.Add(produto);
            appDbContext.SaveChanges();

            produto.Id.Should().BePositive();

            var entrada = auditDbContext.AuditEntries.Single();
            entrada.IDTabelaAfetada.Should().Be(produto.Id.ToString());
            entrada.ValoresNovos.Should().Contain($"\"Id\":{produto.Id}");
        }
    }

}