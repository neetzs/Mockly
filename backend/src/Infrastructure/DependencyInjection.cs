// backend/src/Infrastructure/DependencyInjection.cs
using Mockly.Domain.Interfaces;
using Mockly.Infrastructure.AI;
using Mockly.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mockly.Infrastructure.Persistence;
using Mockly.Infrastructure.Persistence.Seed;
using Mockly.Application.Common.Interfaces;
using Mockly.Infrastructure.Security;
namespace Mockly.Infrastructure;

public static class DependencyInjection
{
    /// Modelo por defecto si no se configura OpenAI:Model: el más económico de OpenAI
    /// (USD 0,10 / 0,50 por millón de tokens de entrada / salida, noviembre 2026).
    private const string DefaultOpenAiModel = "gpt-6-luna";

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


        // Exponer IApplicationDbContext a Application
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Componentes de Seguridad y Tokens
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Motor de evaluación por IA: OpenAI si hay API key configurada, si no el mock.
        // La key llega por configuración y no importa de dónde: user-secrets en desarrollo,
        // variable de entorno OpenAI__ApiKey en Docker/AWS. El código es el mismo en todos lados.
        // El resto de la app depende solo de IInterviewEvaluationService y no se entera del cambio.
        var openAiApiKey = configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(openAiApiKey))
        {
            var model = configuration["OpenAI:Model"] ?? DefaultOpenAiModel;

            // IChatClient es la abstracción de Microsoft.Extensions.AI: mañana se puede cambiar
            // OpenAI por Azure, Ollama u otro proveedor tocando solo esta línea.
            services.AddChatClient(new OpenAIClient(openAiApiKey).GetChatClient(model).AsIChatClient())
                .UseLogging();

            services.AddScoped<IInterviewEvaluationService, OpenAiInterviewEvaluationService>();
            services.AddSingleton(new AiEvaluatorInfo("OpenAI", model));
        }
        else
        {
            services.AddScoped<IInterviewEvaluationService, FakeInterviewEvaluationService>();
            services.AddSingleton(new AiEvaluatorInfo("Evaluador simulado", null));
        }

        // Certificados PDF. QuestPDF exige declarar la licencia: Community es gratuita
        // para proyectos open source y empresas con ingresos < USD 1M (nuestro caso).
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddSingleton<ICertificateGenerator, QuestPdfCertificateGenerator>();

        // Seeder de datos demo (se invoca solo en Development desde Program.cs)
        services.AddScoped<DbSeeder>();

        return services;
    }
}
