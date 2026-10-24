// backend/src/Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockly.Infrastructure.Persistence;

namespace Mockly.Infrastructure;

public static class DependencyInjection
{
    /// Registra la infraestructura de persistencia conectada a PostgreSQL con reintentos automáticos.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection' en la configuración.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Guarda las migraciones dentro del ensamblado Infrastructure
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                // Tolera cortes momentáneos de red reintentando hasta 3 veces
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        return services;
    }
}