// backend/src/Domain/Entities/TechnicalChallenge.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.Domain.Entities;


/// Reto de código que el candidato debe resolver en el editor Monaco, en un lenguaje concreto.
/// Puede pertenecer a una búsqueda laboral o a la biblioteca de práctica (JobOpeningId = null).
public class TechnicalChallenge : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// Código base que se carga inicialmente en el editor interactivo.
    public string InitialCodeTemplate { get; private set; } = string.Empty;

    public ChallengeDifficulty Difficulty { get; private set; }

    /// Lenguaje del reto: define el resaltado del editor y cómo evalúa la IA.
    public ProgrammingLanguage Language { get; private set; }
    /// Null = reto de la biblioteca de práctica, disponible para cualquier candidato.
    public Guid? JobOpeningId { get; private set; }
    public JobOpening? JobOpening { get; private set; }

    /// Propiedad calculada (solo get): EF no la mapea a una columna.
    public bool IsPractice => JobOpeningId is null;

    /// Copia este reto de la biblioteca para una búsqueda. Es una copia (y no una referencia) a propósito:
    /// el reto queda "congelado" y todos los candidatos de la búsqueda se evalúan con la misma consigna,
    /// aunque después se edite el original de la biblioteca.
    public TechnicalChallenge CopyForJobOpening(Guid jobOpeningId)
    {
        if (!IsPractice)
        {
            throw new DomainException("Solo se pueden agregar retos de la biblioteca.");
        }

        return new TechnicalChallenge(Title, Description, InitialCodeTemplate, Difficulty, Language, jobOpeningId);
    }

    private TechnicalChallenge() { }

    public TechnicalChallenge(string title, string description, string initialCodeTemplate, ChallengeDifficulty difficulty, ProgrammingLanguage language, Guid? jobOpeningId)
    {
        Title = title.Trim();
        Description = description.Trim();
        InitialCodeTemplate = initialCodeTemplate;
        Difficulty = difficulty;
        Language = language;
        JobOpeningId = jobOpeningId;
    }
}