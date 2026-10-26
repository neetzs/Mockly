// backend/src/Application/Features/Auth/Services/AuthService.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Auth.DTOs;
using Mockly.Domain.Entities;

namespace Mockly.Application.Features.Auth.Services;

/// Orquesta el registro, validación y verificación de usuarios.
public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"El correo '{request.Email}' ya se encuentra registrado.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(normalizedEmail, passwordHash, request.FullName, request.Role);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales de acceso incorrectas.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, user.Role.ToString(), token);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        return new CurrentUserResponse(user.Id, user.Email, user.FullName, user.Role.ToString());
    }
}