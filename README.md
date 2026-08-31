# AuditLogCM

Biblioteca para auditoria de ações em aplicações .NET, com integração fácil ao Entity Framework Core e suporte a serialização em JSON.

## Instalação

Instale o pacote via NuGet:

```
dotnet add package AuditLogCM
```

ou via Package Manager Console:

```
Install-Package AuditLogCM
```

## Como usar

Exemplo mínimo de configuração no `Program.cs` para aplicações ASP.NET Core com EF Core:

```csharp
using AuditLogCM.EFCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// implemente ICurrentUserResolver de acordo com sua fonte de usuário atual (ex: HttpContext)
// registra o AuditDbContext (onde os logs de auditoria são gravados) e os serviços de suporte
builder.Services.AddAuditLog<MeuCurrentUserResolver>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("AuditConnection")),
    configureOptions: audit =>
    {
        // opcional: entidades que nunca devem gerar auditoria
        audit.Ignorar<MinhaEntidadeSemAuditoria>();
    });

// registre o DbContext da sua aplicação normalmente e associe o interceptor via UseAuditLog
builder.Services.AddDbContext<MeuDbContext>((sp, options) =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseAuditLog(sp));

var app = builder.Build();

app.MapControllers();

app.Run();
```

Para excluir uma propriedade sensível (senha, token, hash) da auditoria, marque-a com `[AuditIgnore]`:

```csharp
using AuditLogCM.Core.Attributes;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    [AuditIgnore]
    public string SenhaHash { get; set; } = string.Empty;
}
```

Consulte os testes em `AuditLogCM.Tests` para exemplos de uso da serialização JSON e do interceptor.

## Funcionalidades

- Interceptação automática de operações do EF Core (síncronas e assíncronas) para gerar logs de auditoria
- Auditoria só é persistida se o SaveChanges principal tiver sucesso (sem registros órfãos em caso de falha)
- Serialização de `AuditEntry` para JSON (via `JsonAuditSerializer`)
- Extensível: interfaces `IAuditSerializer` e `ICurrentUserResolver`
- Armazenamento de entradas de auditoria em banco via `AuditDbContext`
- Exclusão de entidades inteiras da auditoria via `AuditOptions.Ignorar<TEntidade>()`
- Exclusão de propriedades sensíveis via atributo `[AuditIgnore]`
- Suporte a chaves primárias compostas

## Tecnologias

- .NET 9 / C#
- Entity Framework Core
- System.Text.Json para serialização JSON
- xUnit para testes (projeto `AuditLogCM.Tests`)

## Contribuição

Contribuições são bem-vindas — abra issues ou pull requests.

## Licença

Este projeto está licenciado sob os termos da licença MIT — veja o arquivo [LICENSE](LICENSE) para mais detalhes.

