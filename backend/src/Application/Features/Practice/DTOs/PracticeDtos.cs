// backend/src/Application/Features/Practice/DTOs/PracticeDtos.cs
namespace Mockly.Application.Features.Practice.DTOs;

/// Reto de la biblioteca de práctica, con el progreso del candidato logueado.
public sealed record PracticeChallengeResponse(
    Guid Id,
    string Title,
    string Difficulty,
    string Language,
    int Attempts,          // cuántas veces lo intentó (cualquier estado)
    int? BestScore,        // mejor puntaje entre los intentos completados
    Guid? LastAttemptId);  // para retomar o ver el último intento
