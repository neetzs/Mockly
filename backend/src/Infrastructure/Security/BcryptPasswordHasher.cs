// backend/src/Infrastructure/Security/BcryptPasswordHasher.cs
using Mockly.Application.Common.Interfaces;

namespace Mockly.Infrastructure.Security;

/// Provee hashing criptográfico y verificación de contraseñas mediante BCrypt
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // Factor de costo 12: estándar enterprise recomendado para balancear seguridad y CPU
    private const int WorkFactor = 12;

    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);

    public bool VerifyPassword(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
}