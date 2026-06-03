using AuditLogCM.Core.Interfaces;
using AuditLogCM.EFCore.DbContext;
using AuditLogCM.EFCore.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuditLogCM.EFCore.Extensions
{
    public static class AuditLogExtensions
    {
        public static IServiceCollection AddAuditLog<TUserResolver>(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDb)
            where TUserResolver : class, ICurrentUserResolver
        {
            services.AddDbContext<AuditDbContext>(configureDb);
            services.AddScoped<Interceptors.AuditInterceptor>();
            services.AddScoped<ICurrentUserResolver, TUserResolver>();
            services.AddSingleton<IAuditSerializer, JsonAuditSerializer>();

            return services;
        }
       
    }

}
