// backend/src/Infrastructure/Persistence/Seed/DbSeeder.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mockly.Application.Common.Interfaces;
using Mockly.Domain.Common;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;
using Mockly.Domain.Interfaces;
using System.Text.Json;

namespace Mockly.Infrastructure.Persistence.Seed;

/// Carga datos de demostración (usuarios, búsqueda laboral, retos y una entrevista)
/// para que cualquiera del equipo pueda probar la API apenas levanta el proyecto.
/// Se ejecuta en Development o cuando Database:SeedDemoData = true (ver Program.cs).
public sealed class DbSeeder
{
    // Credenciales demo: documentadas en el README para que el equipo pueda loguearse.
    public const string RecruiterEmail = "recruiter@mockly.dev";
    public const string CandidateEmail = "candidate@mockly.dev";
    public const string DemoPassword = "Demo123!";

    // Títulos de las búsquedas demo: también sirven como clave de idempotencia de cada paso.
    private const string DotNetJobTitle = "Backend Developer Jr. (.NET)";
    private const string FullstackJobTitle = "Desarrollador/a Fullstack";

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

        // Paso 8 — Segunda búsqueda con retos en otros lenguajes (Python, TypeScript, Java, SQL).
        // Va después del ranking para poder sumarle entrevistas evaluadas a esos mismos candidatos.
        await SeedMultiLanguageDataAsync(cancellationToken);

        // Paso 9 — Biblioteca de práctica: retos sin búsqueda, disponibles para cualquier candidato registrado.
        await SeedPracticeLibraryAsync(cancellationToken);

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
            DotNetJobTitle,
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
            .FirstAsync(j => j.Recruiter.Email == RecruiterEmail && j.Title == DotNetJobTitle, cancellationToken);
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
            _context.Users.Add(candidate);
            _context.InterviewSessions.Add(CreateCompletedSession(
                candidate.Id, jobOpening.Id, challenges[sample.Challenge].Id, sample.Quality, sample.Solving, sample.Architecture));
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seed de ranking completado: {Count} entrevistas evaluadas de ejemplo.", samples.Length);
    }

    private async Task SeedMultiLanguageDataAsync(CancellationToken cancellationToken)
    {
        // Idempotencia propia: así también se agrega en bases que ya tenían el seed de C#.
        if (await _context.JobOpenings.AnyAsync(j => j.Title == FullstackJobTitle, cancellationToken))
        {
            return;
        }

        var recruiter = await _context.Users.FirstAsync(u => u.Email == RecruiterEmail, cancellationToken);
        var candidate = await _context.Users.FirstAsync(u => u.Email == CandidateEmail, cancellationToken);

        var jobOpening = new JobOpening(
            FullstackJobTitle,
            "Búsqueda fullstack: scripting en Python, frontend en TypeScript, backend en Java y consultas SQL.",
            recruiter.Id);
        var challenges = CreateMultiLanguageChallenges(jobOpening.Id);

        _context.JobOpenings.Add(jobOpening);
        _context.TechnicalChallenges.AddRange(challenges);

        // Una entrevista por reto para el candidato demo, igual que en la búsqueda .NET.
        _context.InterviewSessions.AddRange(
            challenges.Select(challenge => new InterviewSession(candidate.Id, jobOpening.Id, challenge.Id)));

        // Ranking inicial de esta búsqueda: reutilizamos a los candidatos de ejemplo si existen.
        var rankingCandidates = await _context.Users
            .Where(u => u.Email.EndsWith("@ranking.mockly.dev"))
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        // (índice del reto, calidad, resolución, arquitectura) — uno por candidato de ejemplo
        var samples = new (int Challenge, int Quality, int Solving, int Architecture)[]
        {
            (0, 86, 90, 74),   // Python
            (3, 68, 72, 61),   // SQL
            (1, 90, 84, 88),   // TypeScript
        };

        foreach (var (user, sample) in rankingCandidates.Zip(samples))
        {
            _context.InterviewSessions.Add(CreateCompletedSession(
                user.Id, jobOpening.Id, challenges[sample.Challenge].Id, sample.Quality, sample.Solving, sample.Architecture));
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seed multilenguaje completado: {Count} retos en {Job}.", challenges.Count, FullstackJobTitle);
    }

    /// Entrevista ya evaluada para el ranking, respetando las reglas del dominio (Start → Complete).
    private async Task SeedPracticeLibraryAsync(CancellationToken cancellationToken)
    {
        // Idempotencia: si ya hay algún reto sin búsqueda, la biblioteca ya fue cargada.
        if (await _context.TechnicalChallenges.AnyAsync(c => c.JobOpeningId == null, cancellationToken))
        {
            return;
        }

        var library = CreatePracticeLibrary();
        _context.TechnicalChallenges.AddRange(library);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Biblioteca de práctica sembrada: {Count} retos.", library.Count);
    }

    private static InterviewSession CreateCompletedSession(Guid candidateId, Guid jobOpeningId, Guid challengeId, int quality, int solving, int architecture)
    {
        var session = new InterviewSession(candidateId, jobOpeningId, challengeId);

        // Misma regla que usa el motor de IA (ScoreWeights, en el dominio).
        var overall = ScoreWeights.CalculateOverall(solving, quality, architecture);
        var result = new TechnicalScoreResult(
            overall, quality, solving, architecture,
            "Evaluación de ejemplo generada por el seeder.",
            ["Solución correcta para los casos principales."],
            ["Agregar tests unitarios."]);

        session.Start();
        session.Complete("// código de ejemplo del seeder", overall, JsonSerializer.Serialize(result));
        return session;
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
            ProgrammingLanguage.CSharp,
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
            ProgrammingLanguage.CSharp,
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
            ProgrammingLanguage.CSharp,
            jobOpeningId),
    ];
    private static List<TechnicalChallenge> CreateMultiLanguageChallenges(Guid jobOpeningId) =>
    [
        new TechnicalChallenge(
            "Contar palabras",
            """
            Implementá count_words(text) que devuelva un diccionario con la cantidad de apariciones
            de cada palabra, sin distinguir mayúsculas y ignorando signos de puntuación.
            Ejemplo: "Hola, hola mundo" → {"hola": 2, "mundo": 1}.
            """,
            """
            def count_words(text: str) -> dict[str, int]:
                # Tu código acá
                raise NotImplementedError
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.Python,
            jobOpeningId),

        new TechnicalChallenge(
            "Agrupar por propiedad",
            """
            Implementá groupBy(items, key) que agrupe un array de objetos por el valor de una propiedad,
            manteniendo el tipado. Ejemplo: groupBy(users, "role") → { admin: [...], user: [...] }.
            No uses librerías externas.
            """,
            """
            export function groupBy<T, K extends keyof T>(items: T[], key: K): Record<string, T[]> {
              // Tu código acá
              throw new Error("Not implemented");
            }
            """,
            ChallengeDifficulty.MidLevel,
            ProgrammingLanguage.TypeScript,
            jobOpeningId),

        new TechnicalChallenge(
            "Detectar anagramas",
            """
            Implementá isAnagram(a, b) que devuelva true si las dos palabras son anagramas
            (mismas letras con la misma frecuencia), ignorando mayúsculas y espacios.
            Ejemplo: "Roma" y "amor" → true. Apuntá a una solución O(n).
            """,
            """
            public class Solution {
                public static boolean isAnagram(String a, String b) {
                    // Tu código acá
                    throw new UnsupportedOperationException();
                }
            }
            """,
            ChallengeDifficulty.MidLevel,
            ProgrammingLanguage.Java,
            jobOpeningId),

        new TechnicalChallenge(
            "Top clientes por facturación",
            """
            Con las tablas customers(id, name, country) y orders(id, customer_id, total, created_at),
            escribí una consulta que devuelva los 5 clientes con mayor facturación de 2025: nombre, país,
            cantidad de pedidos y total facturado, de mayor a menor. Los clientes sin pedidos en 2025 no aparecen.
            """,
            """
            -- customers(id, name, country)
            -- orders(id, customer_id, total, created_at)
            SELECT
              -- Tu consulta acá
            """,
            ChallengeDifficulty.Senior,
            ProgrammingLanguage.Sql,
            jobOpeningId),
    ];
    // Un reto por lenguaje y nivel variado: alcanza para que un candidato nuevo tenga algo para practicar
    // en cualquier stack apenas se registra. JobOpeningId = null → biblioteca de práctica.
    private static List<TechnicalChallenge> CreatePracticeLibrary() =>
    [
        new TechnicalChallenge(
            "Palíndromo",
            """
            Implementá IsPalindrome(text) que devuelva true si el texto se lee igual al derecho y al revés,
            ignorando mayúsculas, espacios y signos de puntuación.
            Ejemplo: "Anita lava la tina" → true.
            """,
            """
            public static class Solution
            {
                public static bool IsPalindrome(string text)
                {
                    // Tu código acá
                    throw new NotImplementedException();
                }
            }
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.CSharp,
            null),

        new TechnicalChallenge(
            "Caché LRU",
            """
            Implementá una clase LruCache<TKey, TValue> con capacidad fija, Get(key) y Put(key, value),
            ambas en O(1). Cuando se llena, descarta el elemento usado hace más tiempo.
            TryGet devuelve false si la clave no existe.
            """,
            """
            public sealed class LruCache<TKey, TValue> where TKey : notnull
            {
                public LruCache(int capacity)
                {
                    // Tu código acá
                }

                public bool TryGet(TKey key, out TValue? value) => throw new NotImplementedException();

                public void Put(TKey key, TValue value) => throw new NotImplementedException();
            }
            """,
            ChallengeDifficulty.MidLevel,
            ProgrammingLanguage.CSharp,
            null),

        new TechnicalChallenge(
            "Segundo mayor",
            """
            Implementá second_largest(numbers) que devuelva el segundo valor distinto más grande de la lista,
            o None si no existe. Recorré la lista una sola vez.
            Ejemplo: [4, 1, 4, 3] → 3.
            """,
            """
            def second_largest(numbers: list[int]) -> int | None:
                # Tu código acá
                raise NotImplementedError
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.Python,
            null),

        new TechnicalChallenge(
            "Debounce",
            """
            Implementá debounce(fn, ms) que devuelva una función que solo ejecute fn cuando pasaron ms
            milisegundos desde la última llamada. Los argumentos de la última llamada son los que se usan.
            """,
            """
            function debounce(fn, ms) {
              // Tu código acá
            }

            module.exports = { debounce };
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.JavaScript,
            null),

        new TechnicalChallenge(
            "Reintentos con backoff",
            """
            Implementá retry(task, attempts, baseDelayMs) que ejecute una función async y, si falla,
            la reintente hasta attempts veces esperando baseDelayMs * 2^intento entre cada reintento.
            Si se agotan los intentos, relanzá el último error.
            """,
            """
            export async function retry<T>(
              task: () => Promise<T>,
              attempts: number,
              baseDelayMs: number,
            ): Promise<T> {
              // Tu código acá
              throw new Error("No implementado");
            }
            """,
            ChallengeDifficulty.MidLevel,
            ProgrammingLanguage.TypeScript,
            null),

        new TechnicalChallenge(
            "Validar email",
            """
            Implementá isValidEmail(email) sin usar expresiones regulares: una sola "@", parte local
            y dominio no vacíos, y el dominio con al menos un punto que no esté al principio ni al final.
            """,
            """
            public class Solution {
                public static boolean isValidEmail(String email) {
                    // Tu código acá
                    return false;
                }
            }
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.Java,
            null),

        new TechnicalChallenge(
            "Clientes sin pedidos",
            """
            Con las tablas customers(id, name) y orders(id, customer_id, total, created_at),
            escribí una consulta que liste los clientes que no hicieron ningún pedido en los últimos 30 días,
            ordenados por nombre.
            """,
            """
            -- Tu consulta acá
            SELECT
            """,
            ChallengeDifficulty.Junior,
            ProgrammingLanguage.Sql,
            null),
    ];
}

public static class DbSeederExtensions
{
    /// Aplica las migraciones pendientes y/o carga los datos demo.
    /// - Migrar: crea/actualiza las tablas (equivale a `dotnet ef database update`).
    ///   Es lo que permite que el contenedor arranque contra una base vacía sin pasos manuales.
    /// - Sembrar: carga usuarios, retos y entrevistas demo (idempotente).
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, bool migrate, bool seed, CancellationToken cancellationToken = default)
    {
        // El DbContext es Scoped (uno por request). Al arrancar no hay request,
        // así que creamos un scope a mano y lo descartamos al terminar (using).
        using var scope = services.CreateScope();

        if (migrate)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync(cancellationToken);
        }

        if (seed)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
