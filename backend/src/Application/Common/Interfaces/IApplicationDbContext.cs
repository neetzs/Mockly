// backend/src/Application/Common/Interfaces/IApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Mockly.Domain.Entities;

namespace Mockly.Application.Common.Interfaces;

/// <summary>
/// Abstracción del contexto de persistencia consumida por la capa Application.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<JobOpening> JobOpenings { get; }
    DbSet<TechnicalChallenge> TechnicalChallenges { get; }
    DbSet<InterviewSession> InterviewSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}