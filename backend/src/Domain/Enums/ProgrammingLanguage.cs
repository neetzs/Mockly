// backend/src/Domain/Enums/ProgrammingLanguage.cs
namespace Mockly.Domain.Enums;

/// Lenguaje en el que se resuelve un reto. Define el resaltado del editor y el contexto que recibe la IA.
/// Los valores numéricos se guardan en la base: no reordenar, solo agregar al final.
public enum ProgrammingLanguage
{
    CSharp = 1,
    Python = 2,
    JavaScript = 3,
    TypeScript = 4,
    Java = 5,
    Sql = 6
}

public static class ProgrammingLanguageExtensions
{
    /// Nombre legible para mostrar (certificado, prompt de la IA). El frontend tiene su propio mapeo.
    public static string ToDisplayName(this ProgrammingLanguage language) => language switch
    {
        ProgrammingLanguage.CSharp => "C#",
        ProgrammingLanguage.Python => "Python",
        ProgrammingLanguage.JavaScript => "JavaScript",
        ProgrammingLanguage.TypeScript => "TypeScript",
        ProgrammingLanguage.Java => "Java",
        ProgrammingLanguage.Sql => "SQL",
        _ => language.ToString()
    };
}
