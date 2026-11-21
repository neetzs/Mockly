// backend/src/Domain/Entities/TechnicalChallenge.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;

namespace Mockly.Domain.Entities;


/// Reto de código que el candidato debe resolver en el editor Monaco, en un lenguaje concreto.
public class TechnicalChallenge : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// Código base que se carga inicialmente en el editor interactivo.
    public string InitialCodeTemplate { get; private set; } = string.Empty;

    public ChallengeDifficulty Difficulty { get; private set; }

    /// Lenguaje del reto: define el resaltado del editor y cómo evalúa la IA.
    public ProgrammingLanguage Language { get; private set; }
    public Guid JobOpeningId { get; private set; }
    public JobOpening JobOpening { get; private set; } = null!;

    private TechnicalChallenge() { }

    public TechnicalChallenge(string title, string description, string initialCodeTemplate, ChallengeDifficulty difficulty, ProgrammingLanguage language, Guid jobOpeningId)
    {
        Title = title.Trim();
        Description = description.Trim();
        InitialCodeTemplate = initialCodeTemplate;
        Difficulty = difficulty;
        Language = language;
        JobOpeningId = jobOpeningId;
    }
}