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
using AuditLogCM.EFCore;

var builder = WebApplication.CreateBuilder(args);

// registrar DbContext normalmente
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// registrar serviços de auditoria (exemplo)
builder.Services.AddAuditLogging();

var app = builder.Build();

app.MapControllers();

app.Run();
```

Para gravar entradas de auditoria manualmente você pode usar o `AuditDbContext` ou os serviços expostos pelo pacote, por exemplo:

```csharp
var auditor = app.Services.GetRequiredService<IAuditSerializer>();
// construir AuditEntry e salvar via DbContext
```

Consulte os testes em `AuditLogCM.Tests` para exemplos de uso da serialização JSON.

## Funcionalidades

- Interceptação automática de operações do EF Core para gerar logs de auditoria
- Serialização de `AuditEntry` para JSON (via `JsonAuditSerializer`)
- Extensível: interface `IAuditSerializer` e `ICurrentUserResolver`
- Armazenamento de entradas de auditoria em banco via `AuditDbContext`
- Filtros e extensões para configurar quais entidades e ações devem ser auditadas

## Tecnologias

- .NET 9 / C#
- Entity Framework Core
- System.Text.Json para serialização JSON
- xUnit para testes (projeto `AuditLogCM.Tests`)

## Contribuição

Contribuições são bem-vindas — abra issues ou pull requests.

## Licença

