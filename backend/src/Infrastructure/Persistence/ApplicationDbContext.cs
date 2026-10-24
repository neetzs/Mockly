// backend/src/Infrastructure/Persistence/ApplicationDbContext.cs
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Mockly.Domain.Entities;

namespace Mockly.Infrastructure.Persistence;

/// Punto de entrada principal de Entity Framework Core 10 para la persistencia en PostgreSQL.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<JobOpening> JobOpenings => Set<JobOpening>();
    public DbSet<TechnicalChallenge> TechnicalChallenges => Set<TechnicalChallenge>();
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones IEntityTypeConfiguration presentes en este ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}