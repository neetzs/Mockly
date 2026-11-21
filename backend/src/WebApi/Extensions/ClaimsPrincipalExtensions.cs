// backend/src/WebApi/Extensions/ClaimsPrincipalExtensions.cs
using System.Security.Claims;

namespace Mockly.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// Devuelve el id del usuario autenticado leyendo el claim "sub" del JWT.
    /// El handler de JWT puede mapear "sub" a ClaimTypes.NameIdentifier, por eso buscamos ambos.
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("El token no contiene un identificador de usuario válido.");
    }
}
