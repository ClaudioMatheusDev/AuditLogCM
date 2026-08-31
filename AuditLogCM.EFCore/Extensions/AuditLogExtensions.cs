using AuditLogCM.Core.Configuration;
using AuditLogCM.Core.Interfaces;
using AuditLogCM.EFCore.Interceptors;
using AuditLogCM.EFCore.Persistence;
using AuditLogCM.EFCore.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuditLogCM.EFCore.Extensions
{
    public static class AuditLogExtensions
    {
        /// <summary>
        /// Registra os serviços de auditoria: o <see cref="AuditDbContext"/> (onde os logs são gravados),
        /// o <see cref="AuditInterceptor"/> e o resolvedor de usuário atual.
        /// Para o interceptor efetivamente capturar as mudanças, associe-o ao DbContext da sua aplicação
        /// usando <see cref="UseAuditLog"/> ao configurá-lo.
        /// </summary>
        public static IServiceCollection AddAuditLog<TUserResolver>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder> configureAuditDb,
            Action<AuditOptions>? configureOptions = null)
            where TUserResolver : class, ICurrentUserResolver
        {
            var opcoes = new AuditOptions();
            configureOptions?.Invoke(opcoes);

            services.AddDbContext<AuditDbContext>(configureAuditDb);
            services.AddSingleton(opcoes);
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<ICurrentUserResolver, TUserResolver>();
            services.AddSingleton<IAuditSerializer, JsonAuditSerializer>();

            return services;
        }

        /// <summary>
        /// Associa o <see cref="AuditInterceptor"/> ao DbContext sendo configurado, para que suas
        /// operações de SaveChanges passem a gerar registros de auditoria.
        /// </summary>
        public static DbContextOptionsBuilder UseAuditLog(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
        {
            var interceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
            return optionsBuilder.AddInterceptors(interceptor);
        }
    }
}
