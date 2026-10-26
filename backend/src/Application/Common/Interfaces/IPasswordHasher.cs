// backend/src/Application/Common/Interfaces/IPasswordHasher.cs
namespace Mockly.Application.Common.Interfaces;

/// Contrato para hasheo seguro y verificación de contraseñas.
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}