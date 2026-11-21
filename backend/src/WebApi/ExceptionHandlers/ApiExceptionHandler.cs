// backend/src/WebApi/ExceptionHandlers/ApiExceptionHandler.cs
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mockly.Application.Common.Exceptions;
using Mockly.Domain.Exceptions;

namespace Mockly.WebApi.ExceptionHandlers;

/// Traduce las excepciones de la Application en respuestas HTTP con el código correcto.
public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1) Según QUÉ excepción sea, elijo status + título + detalle
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                string.Join(" | ", ex.Errors.Select(e => e.ErrorMessage))),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                exception.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "No autorizado",
                exception.Message),

            ForbiddenAccessException => (
                StatusCodes.Status403Forbidden,
                "Acceso denegado",
                exception.Message),

            DomainException => (
                StatusCodes.Status409Conflict,
                "Operación no permitida",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado. Intente nuevamente.")
        };

        // 2) Registro en el log (4xx = warning, 5xx = error)
        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "Respuesta {StatusCode}: {Message}", statusCode, exception.Message);
        }

        // 3) Escribo la respuesta JSON con el formato ProblemDetails
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        // 4) true = "yo lo manejo, no sigas buscando otros manejadores"
        return true;
    }
}
