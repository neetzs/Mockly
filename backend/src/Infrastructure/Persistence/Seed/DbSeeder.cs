// backend/src/Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockly.Application.Common.Interfaces;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;
using Mockly.Domain.Interfaces;
using System.Text.Json;

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
        if (!await _context.Users.AnyAsync(u => u.Email == CandidateEmail, cancellationToken))
        {
            await SeedBaseDataAsync(cancellationToken);
        }

        // Paso 7 — Candidatos con entrevistas ya evaluadas para que el ranking del dashboard no arranque vacío.
        // Tiene su propio chequeo de idempotencia, así también se agrega en bases sembradas antes.
        await SeedRankingDataAsync(cancellationToken);

        await LogDemoSessionsAsync(cancellationToken);
    }

    private async Task SeedBaseDataAsync(CancellationToken cancellationToken)
    {

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
    }

    private async Task SeedRankingDataAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email.EndsWith("@ranking.mockly.dev"), cancellationToken))
        {
            return;
        }

        // Reutilizamos la búsqueda y los retos del reclutador demo.
        var jobOpening = await _context.JobOpenings
            .Include(j => j.Challenges)
            .FirstAsync(j => j.Recruiter.Email == RecruiterEmail, cancellationToken);
        var challenges = jobOpening.Challenges.OrderBy(c => c.Difficulty).ToList();

        var passwordHash = _passwordHasher.HashPassword(DemoPassword);

        // (nombre, email, índice del reto, calidad, resolución, arquitectura)
        var samples = new (string Name, string Email, int Challenge, int Quality, int Solving, int Architecture)[]
        {
            ("Ana Torres", "ana@ranking.mockly.dev", 2, 92, 88, 85),
            ("Bruno Díaz", "bruno@ranking.mockly.dev", 1, 70, 64, 58),
            ("Lucía Fernández", "lucia@ranking.mockly.dev", 0, 81, 95, 72),
        };

        foreach (var sample in samples)
        {
            var candidate = new User(sample.Email, passwordHash, sample.Name, UserRole.Candidate);
            var session = new InterviewSession(candidate.Id, jobOpening.Id, challenges[sample.Challenge].Id);

            // Mismo ponderado que le pedimos a la IA: 50% resolución, 30% calidad, 20% arquitectura.
            var overall = (int)Math.Round(sample.Solving * 0.5 + sample.Quality * 0.3 + sample.Architecture * 0.2);
            var result = new TechnicalScoreResult(
                overall, sample.Quality, sample.Solving, sample.Architecture,
                "Evaluación de ejemplo generada por el seeder.",
                ["Solución correcta para los casos principales."],
                ["Agregar tests unitarios."]);

            // Respetamos las reglas del dominio: primero Start(), después Complete().
            session.Start();
            session.Complete("// código de ejemplo del seeder", overall, JsonSerializer.Serialize(result));

            _context.Users.Add(candidate);
            _context.InterviewSessions.Add(session);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seed de ranking completado: {Count} entrevistas evaluadas de ejemplo.", samples.Length);
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
