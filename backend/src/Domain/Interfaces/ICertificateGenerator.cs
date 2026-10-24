// backend/src/Domain/Interfaces/ICertificateGenerator.cs
namespace Mockly.Domain.Interfaces;

/// Contrato para la generación de reportes y diplomas en PDF con QuestPDF.
public interface ICertificateGenerator
{
    byte[] GeneratePdfCertificate(
        string candidateName,
        string jobTitle,
        int overallScore,
        DateTime completedAtUtc,
        TechnicalScoreResult evaluationDetails);
}