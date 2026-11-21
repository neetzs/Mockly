// backend/src/Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockly.Application.Common.Interfaces;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;

namespace Mockly.Infrastructure.Persistence.Seed;

/// Carga datos de demostración (usuarios, búsqueda laboral, retos y una entrevista)
/// para que cualquiera del equipo pueda probar la API apenas levanta el proyecto.
/// Solo debe ejecutarse en Development (ver Program.cs).
public sealed class DbSeeder
{
    // Credenciales demo: documentadas en el README para que el equipo pueda loguearse.
    public const string RecruiterEmail = "recruiter@mockly.dev";
    public const string CandidateEmail = "candidate@mockly.dev";
    public const string DemoPassword = "Demo123!";

    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger<DbSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Paso 1 — Idempotencia: si el candidato demo ya existe, la base ya fue sembrada.
        // Chequeamos por email (y no "¿hay algún usuario?") porque alguien pudo haberse
        // registrado a mano antes y igual queremos tener los datos demo.
        if (await _context.Users.AnyAsync(u => u.Email == CandidateEmail, cancellationToken))
        {
            await LogDemoSessionsAsync(cancellationToken);
            return;
        }

        // Paso 2 — Usuarios. La contraseña se hashea con el mismo BCrypt que usa el registro;
        // si guardáramos texto plano, el login fallaría al verificar el hash.
        var passwordHash = _passwordHasher.HashPassword(DemoPassword);
        var recruiter = new User(RecruiterEmail, passwordHash, "Rita Recruiter", UserRole.Recruiter);
        var candidate = new User(CandidateEmail, passwordHash, "Carlos Candidato", UserRole.Candidate);

        // Paso 3 — Búsqueda laboral. BaseEntity genera el Id en el constructor (Guid.NewGuid()),
        // así que recruiter.Id ya existe ANTES de guardar: podemos encadenar las FK en memoria.
        var jobOpening = new JobOpening(
            "Backend Developer Jr. (.NET)",
            "Búsqueda de desarrollador/a backend junior con conocimientos de C#, LINQ y estructuras de datos.",
            recruiter.Id);

        // Paso 4 — Retos técnicos asociados a la búsqueda.
        var challenges = CreateChallenges(jobOpening.Id);

        // Paso 5 — Una entrevista por reto para el candidato demo
        // (así se pueden probar varios submits sin "gastar" la única sesión).
        var sessions = challenges
            .Select(challenge => new InterviewSession(candidate.Id, jobOpening.Id, challenge.Id))
            .ToList();

        // Paso 6 — Agregar todo y guardar UNA sola vez: EF lo ejecuta en una transacción
        // y ordena los INSERT respetando las claves foráneas (primero users, después job, etc.).
        _context.Users.AddRange(recruiter, candidate);
        _context.JobOpenings.Add(jobOpening);
        _context.TechnicalChallenges.AddRange(challenges);
        _context.InterviewSessions.AddRange(sessions);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seed completado: usuarios {Recruiter} y {Candidate} (password: {Password}).",
            RecruiterEmail, CandidateEmail, DemoPassword);

        await LogDemoSessionsAsync(cancellationToken);
    }

    /// Imprime en consola los ids de las entrevistas demo, para copiarlos en Scalar / WebApi.http.
    private async Task LogDemoSessionsAsync(CancellationToken cancellationToken)
    {
        var sessions = await _context.InterviewSessions
            .AsNoTracking()
            .Where(s => s.Candidate.Email == CandidateEmail)
            .Select(s => new { s.Id, s.Status, s.TechnicalChallenge.Title })
            .ToListAsync(cancellationToken);

        foreach (var s in sessions)
        {
            _logger.LogInformation("Entrevista demo {Id} [{Status}] → {Title}", s.Id, s.Status, s.Title);
        }
    }

    private static List<TechnicalChallenge> CreateChallenges(Guid jobOpeningId) =>
    [
        new TechnicalChallenge(
            "FizzBuzz",
            """
            Implementá el método FizzBuzz(n) que devuelva una lista con los números del 1 al n,
            reemplazando los múltiplos de 3 por "Fizz", los de 5 por "Buzz" y los de ambos por "FizzBuzz".
            """,
            """
            public static class Solution
            {
                public static List<string> FizzBuzz(int n)
                {
                    // Tu código acá
                    throw new NotImplementedException();
                }
            }
            """,
            ChallengeDifficulty.Junior,
            jobOpeningId),

        new TechnicalChallenge(
            "Invertir un string",
            """
            Implementá Reverse(text) que devuelva el string invertido SIN usar Array.Reverse,
            Enumerable.Reverse ni LINQ. Pensá en la complejidad temporal y espacial de tu solución.
            """,
            """
            public static class Solution
            {
                public static string Reverse(string text)
                {
                    // Tu código acá
                    throw new NotImplementedException();
                }
            }
            """,
            ChallengeDifficulty.MidLevel,
            jobOpeningId),

        new TechnicalChallenge(
            "Paréntesis balanceados",
            """
            Implementá IsBalanced(expression) que devuelva true si los caracteres (), [] y {}
            están correctamente balanceados y anidados. Ejemplos: "([]{})" → true, "([)]" → false.
            Pista: una pila (Stack<char>) resuelve el problema en O(n).
            """,
            """
            public static class Solution
            {
                public static bool IsBalanced(string expression)
                {
                    // Tu código acá
                    throw new NotImplementedException();
                }
            }
            """,
            ChallengeDifficulty.Senior,
            jobOpeningId),
    ];
}

/// Extensión para ejecutar el seeder desde Program.cs sin exponer detalles de EF.
public static class DbSeederExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // El DbContext es Scoped (uno por request). Al arrancar no hay request,
        // así que creamos un scope a mano y lo descartamos al terminar (using).
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
