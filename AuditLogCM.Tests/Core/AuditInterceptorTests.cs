using AuditLogCM.Core.Interfaces;
using AuditLogCM.EFCore.DbContext;
using AuditLogCM.EFCore.Interceptors;
using AuditLogCM.EFCore.Serializers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AuditLogCM.Tests.Core
{   
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Produto> Produtos { get; set; } = null!;

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
    }

}