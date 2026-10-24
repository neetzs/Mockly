using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mockly.Infrastructure.Persistence;

/// Permite a 'dotnet-ef' instanciar el contexto durante el diseño de migraciones
/// sin depender de la inicialización completa del servidor WebApi
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Cadena de conexión directa al contenedor PostgreSQL local
        const string connectionString = "Host=localhost;Port=5432;Database=mockly_db;Username=mockly_admin;Password=mockly_secure_password_2026";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}