// backend/src/Domain/Entities/TechnicalChallenge.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;

namespace Mockly.Domain.Entities;


/// Reto de código que el candidato debe resolver en el editor Monaco.
public class TechnicalChallenge : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// Código base que se carga inicialmente en el editor interactivo.
    public string InitialCodeTemplate { get; private set; } = string.Empty;

    public ChallengeDifficulty Difficulty { get; private set; }
    public Guid JobOpeningId { get; private set; }
    public JobOpening JobOpening { get; private set; } = null!;

    private TechnicalChallenge() { }

    public TechnicalChallenge(string title, string description, string initialCodeTemplate, ChallengeDifficulty difficulty, Guid jobOpeningId)
    {
        Title = title.Trim();
        Description = description.Trim();
        InitialCodeTemplate = initialCodeTemplate;
        Difficulty = difficulty;
        JobOpeningId = jobOpeningId;
    }
}