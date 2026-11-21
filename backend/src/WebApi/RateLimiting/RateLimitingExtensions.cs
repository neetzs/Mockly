// backend/src/WebApi/RateLimiting/RateLimitingExtensions.cs
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mockly.WebApi.RateLimiting;

/// Nombres de las políticas, para no repetir strings sueltos en los endpoints.
public static class RateLimitPolicies
{
    /// Envíos de código: es el único endpoint que llama a la IA y por lo tanto el único que cuesta plata.
    public const string AiEvaluation = "ai-evaluation";
}

public static class RateLimitingExtensions
{
    /// Rate limiting con el middleware que trae ASP.NET Core (System.Threading.RateLimiting).
    /// Protege la API key de OpenAI: sin esto, alguien podría mandar código en un bucle y vaciar el crédito.
    ///
    /// Configurable por appsettings o variables de entorno (RateLimiting__AiEvaluation__PermitLimit, etc.):
    ///   RateLimiting:AiEvaluation:PermitLimit    envíos permitidos por ventana (default 5)
    ///   RateLimiting:AiEvaluation:WindowSeconds  duración de la ventana en segundos (default 60)
    public static IServiceCollection AddMocklyRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:AiEvaluation:PermitLimit", 5);
        var window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:AiEvaluation:WindowSeconds", 60));

        services.AddRateLimiter(options =>
        {
            // Ventana fija POR USUARIO: cada candidato tiene su propio cupo, así uno no bloquea a los demás.
            // La partición sale del id del JWT; si no hubiera usuario (no debería pasar: el endpoint exige login)
            // se usa la IP como respaldo.
            options.AddPolicy(RateLimitPolicies.AiEvaluation, httpContext =>
            {
                var partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? httpContext.User.FindFirstValue("sub")
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0, // sin cola: si se pasó del límite, se rechaza al instante
                });
            });

            // Respuesta 429 con el mismo formato ProblemDetails que el resto de la API,
            // así el frontend muestra el mensaje sin código especial.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var detail = "Hiciste muchos envíos seguidos. Esperá un momento antes de volver a enviar tu código.";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
                    context.HttpContext.Response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
                    detail = $"Hiciste muchos envíos seguidos. Podés volver a enviar en {seconds} segundos.";
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Demasiados envíos",
                    Detail = detail,
                }, cancellationToken);
            };
        });

        return services;
    }
}
