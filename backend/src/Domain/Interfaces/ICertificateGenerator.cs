// backend/src/Domain/Interfaces/ICertificateGenerator.cs
namespace Mockly.Domain.Interfaces;

/// Datos que se imprimen en el certificado. Un record (en vez de muchos parámetros sueltos)
/// permite sumar campos más adelante sin romper la firma del contrato.
public sealed record CertificateData(
    Guid CertificateId,          // = id de la sesión; sirve como código de verificación
    string CandidateName,
    string JobTitle,
    string ChallengeTitle,
    string Difficulty,
    string Language,             // nombre legible: "C#", "Python", "SQL"...
    DateTime CompletedAtUtc,
    TechnicalScoreResult Evaluation
);

/// Contrato para la generación de certificados de rendimiento en PDF (implementado con QuestPDF).
public interface ICertificateGenerator
{
    /// Devuelve el PDF en memoria, listo para enviarse como respuesta HTTP.
    byte[] GeneratePdfCertificate(CertificateData data);
}
