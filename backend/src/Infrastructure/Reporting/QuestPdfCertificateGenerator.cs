// backend/src/Infrastructure/Reporting/QuestPdfCertificateGenerator.cs
using System.Globalization;
using Mockly.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Mockly.Infrastructure.Reporting;

/// Genera el certificado de rendimiento con QuestPDF (API fluida: el PDF se describe con código,
/// como si fuera un layout de contenedores — columnas, filas, bordes, padding).
/// Es stateless (no guarda estado entre llamadas), por eso se registra como Singleton.
public sealed class QuestPdfCertificateGenerator : ICertificateGenerator
{
    private static readonly CultureInfo SpanishCulture = new("es-AR");

    // Paleta alineada con el frontend (indigo/slate).
    private const string Primary = "#4F46E5";
    private const string Dark = "#0F172A";
    private const string Muted = "#64748B";
    private const string Light = "#E2E8F0";

    public byte[] GeneratePdfCertificate(CertificateData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.PageColor(Colors.White);
                // Lato viene embebida en QuestPDF. Desde 2026.9 NO se usan fuentes del sistema:
                // una fuente faltante corta la generación (así el PDF sale igual en tu PC, Docker o AWS).
                page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(11).FontColor(Dark));

                // Marco doble: borde exterior de color + contenido con padding interno.
                page.Content()
                    .Border(3).BorderColor(Primary)
                    .Padding(6)
                    .Border(1).BorderColor(Light)
                    .PaddingVertical(30).PaddingHorizontal(50)
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Spacing(14);

                        ComposeHeader(column);
                        ComposeBody(column, data);
                        ComposeScores(column, data.Evaluation);
                        ComposeFooter(column, data);
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(ColumnDescriptor column)
    {
        column.Item().AlignCenter().Text(text =>
        {
            text.Span("Mockly").FontSize(20).Bold().FontColor(Dark);
            text.Span("  AI").FontSize(20).Bold().FontColor(Primary);
        });

        column.Item().AlignCenter()
            .Text("CERTIFICADO DE EVALUACIÓN TÉCNICA")
            .FontSize(13).SemiBold().LetterSpacing(0.15f).FontColor(Muted);
    }

    private static void ComposeBody(ColumnDescriptor column, CertificateData data)
    {
        column.Item().PaddingTop(10).AlignCenter().Text("Se certifica que").FontSize(12).FontColor(Muted);

        column.Item().AlignCenter().Text(data.CandidateName).FontSize(30).Bold().FontColor(Dark);

        column.Item().AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(style => style.FontSize(12).FontColor(Muted));
            text.Span("completó el reto ");
            text.Span($"“{data.ChallengeTitle}”").SemiBold().FontColor(Dark);
            text.Span($" en {data.Language} (nivel {data.Difficulty}) para la búsqueda ");
            text.Span(data.JobTitle).SemiBold().FontColor(Dark);
            text.Span(", evaluado por el motor de IA de Mockly.");
        });
    }

    private static void ComposeScores(ColumnDescriptor column, TechnicalScoreResult evaluation)
    {
        column.Item().PaddingTop(8).Row(row =>
        {
            row.Spacing(14);

            // Puntaje general destacado
            row.ConstantItem(150)
                .Background(Primary).Padding(14).AlignMiddle()
                .Column(c =>
                {
                    c.Item().AlignCenter().Text(evaluation.OverallScore.ToString()).FontSize(42).Bold().FontColor(Colors.White);
                    c.Item().AlignCenter().Text("PUNTAJE GENERAL / 100").FontSize(8).SemiBold().FontColor(Colors.White);
                });

            // Desglose por criterio
            row.RelativeItem().Column(c =>
            {
                c.Spacing(8);
                ScoreBar(c, "Resolución del problema", evaluation.ProblemSolvingScore);
                ScoreBar(c, "Calidad de código", evaluation.CodeQualityScore);
                ScoreBar(c, "Arquitectura", evaluation.ArchitectureScore);
            });
        });

        column.Item().PaddingTop(4).Text(evaluation.Summary).FontSize(10).Italic().FontColor(Muted);
    }

    /// Barra horizontal hecha con dos celdas proporcionales: la parte llena y la vacía.
    private static void ScoreBar(ColumnDescriptor column, string label, int score)
    {
        var value = Math.Clamp(score, 0, 100);

        column.Item().Column(c =>
        {
            c.Item().Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(10);
                r.AutoItem().Text(value.ToString()).FontSize(10).Bold();
            });

            c.Item().PaddingTop(3).Height(6).Row(r =>
            {
                if (value > 0) r.RelativeItem(value).Background(Primary);
                if (value < 100) r.RelativeItem(100 - value).Background(Light);
            });
        });
    }

    private static void ComposeFooter(ColumnDescriptor column, CertificateData data)
    {
        column.Item().PaddingTop(12).LineHorizontal(1).LineColor(Light);

        column.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Fecha de emisión").FontSize(8).FontColor(Muted);
                c.Item().Text(FormatDate(data.CompletedAtUtc)).FontSize(10).SemiBold();
            });

            row.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text("Código de verificación").FontSize(8).FontColor(Muted);
                c.Item().AlignRight().Text(data.CertificateId.ToString().ToUpperInvariant()).FontSize(9).LetterSpacing(0.05f);
            });
        });
    }

    /// Las fechas se guardan en UTC; para el certificado las mostramos en hora de Argentina.
    private static string FormatDate(DateTime utc)
    {
        var local = TimeZoneInfo.TryFindSystemTimeZoneById("America/Argentina/Buenos_Aires", out var zone)
            ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone)
            : utc;

        return local.ToString("d 'de' MMMM 'de' yyyy", SpanishCulture);
    }
}
