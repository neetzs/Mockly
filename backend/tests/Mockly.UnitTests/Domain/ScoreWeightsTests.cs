using Mockly.Domain.Common;

namespace Mockly.UnitTests.Domain;

public class ScoreWeightsTests
{
    [Fact]
    public void Weights_SumTo100()
    {
        Assert.Equal(100, ScoreWeights.ProblemSolving + ScoreWeights.CodeQuality + ScoreWeights.Architecture);
    }

    [Theory]
    [InlineData(85, 82, 72, 82)]   // 81,5 → redondea hacia arriba (ejemplo del comentario de ScoreWeights)
    [InlineData(100, 100, 100, 100)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(100, 0, 0, 50)]    // solo resolución del problema
    [InlineData(0, 100, 0, 30)]    // solo calidad de código
    [InlineData(0, 0, 100, 20)]    // solo arquitectura
    [InlineData(81, 80, 80, 81)]   // 80,5 → 81
    [InlineData(80, 81, 80, 80)]   // 80,3 → 80
    public void CalculateOverall_ReturnsWeightedAverageRoundedHalfUp(int problemSolving, int codeQuality, int architecture, int expected)
    {
        Assert.Equal(expected, ScoreWeights.CalculateOverall(problemSolving, codeQuality, architecture));
    }
}
