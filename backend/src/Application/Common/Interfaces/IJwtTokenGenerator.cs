// backend/src/Application/Common/Interfaces/IJwtTokenGenerator.cs
using Mockly.Domain.Entities;

namespace Mockly.Application.Common.Interfaces;

/// Contrato para la emisión de tokens JWT tipados con claims de rol.
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}