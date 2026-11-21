// backend/src/WebApi/Program.cs
using Mockly.WebApi.ExceptionHandlers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mockly.Application;
using Mockly.Infrastructure;
using Mockly.Infrastructure.Persistence.Seed;
using Mockly.Application.Common.Interfaces;
using Mockly.WebApi.Endpoints;
using Mockly.WebApi.Hubs;
using Mockly.WebApi.RateLimiting;
using Mockly.Infrastructure.AI;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Inyección de dependencias de Application e Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. CORS para desarrollo con Vite (Frontend en localhost:5173)
const string frontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Autenticación y Autorización JWT Bearer
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? "clave_super_secreta_mockly_desarrollo_2026_segura_32_bytes";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MocklyApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MocklyClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // SignalR: el navegador NO puede mandar el header Authorization al abrir un WebSocket,
        // así que el cliente envía el JWT en la query (?access_token=...). Lo aceptamos
        // SOLO para las rutas de hubs, para no abrir esa puerta en el resto de la API.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddMocklyRateLimiting(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// SignalR (tiempo real) + implementación del notificador que usa Application
builder.Services.AddSignalR();
builder.Services.AddScoped<IInterviewNotifier, SignalRInterviewNotifier>();

// 4. OpenAPI Nativo de .NET 10
builder.Services.AddOpenApi();

var app = builder.Build();

// Deja en el log qué motor de IA quedó activo: confirma de un vistazo si el deploy tomó la API key.
app.Logger.LogInformation("Motor de evaluación por IA: {Evaluator}", app.Services.GetRequiredService<AiEvaluatorInfo>());

// 5. Exponer OpenAPI y Scalar en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Mockly AI API Reference")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Preparar la base al arrancar. En Development siempre; en otros entornos (p. ej. el contenedor
// de docker-compose) solo si se activa por configuración: Database__MigrateOnStartup=true, etc.
// En producción real conviene dejarlos en false y migrar como paso explícito del deploy.
var isDevelopment = app.Environment.IsDevelopment();
await app.Services.InitializeDatabaseAsync(
    migrate: isDevelopment || app.Configuration.GetValue<bool>("Database:MigrateOnStartup"),
    seed: isDevelopment || app.Configuration.GetValue<bool>("Database:SeedDemoData"));

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors(frontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

// Va DESPUÉS de autenticación: la política particiona por usuario y necesita saber quién es.
app.UseRateLimiter();

// 6. Mapear endpoints mínimos de Auth
app.MapAuthEndpoints();

// 7. Mapear endpoints de la sala de entrevista (Bloque 2)
app.MapInterviewEndpoints();

// 8. Dashboard del reclutador: endpoints REST + hub de SignalR (solo Recruiters)
app.MapRecruiterEndpoints();
app.MapHub<InterviewHub>("/hubs/interviews")
   .RequireAuthorization(policy => policy.RequireRole(nameof(Mockly.Domain.Enums.UserRole.Recruiter)));

app.Run();
