// backend/src/Application/Features/Auth/DTOs/AuthDtos.cs
using Mockly.Domain.Enums;

namespace Mockly.Application.Features.Auth.DTOs;


/// Solicitud de registro de nuevo usuario con rol asignado
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    UserRole Role
);

/// Solicitud de login con credenciales
public sealed record LoginRequest(
    string Email,
    string Password
);


/// Respuesta devuelta tras autenticación o registro exitoso
public sealed record AuthResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string Token
);


/// Respuesta del perfil del usuario actual autenticado
public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role
);