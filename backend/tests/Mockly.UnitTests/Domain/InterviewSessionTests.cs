using Mockly.Domain.Entities;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.UnitTests.Domain;

public class InterviewSessionTests
{
    private static InterviewSession NewSession() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    private static TechnicalChallenge LibraryChallenge() =>
        new("Reto", "Descripción", "// código", ChallengeDifficulty.Junior, ProgrammingLanguage.CSharp, jobOpeningId: null);

    [Fact]
    public void NewSession_IsNotStarted()
    {
        Assert.Equal(InterviewStatus.NotStarted, NewSession().Status);
    }

    [Fact]
    public void Start_MovesToInProgress_AndIsIdempotent()
    {
        var session = NewSession();

        session.Start();
        session.Start(); // recargar la sala no debe fallar

        Assert.Equal(InterviewStatus.InProgress, session.Status);
    }

    [Fact]
    public void Complete_StoresResult_AndClampsScore()
    {
        var session = NewSession();
        session.Start();

        session.Complete("var x = 1;", 150, "{}");

        Assert.Equal(InterviewStatus.Completed, session.Status);
        Assert.Equal(100, session.OverallScore);
        Assert.NotNull(session.CompletedAtUtc);
    }

    [Fact]
    public void Complete_WithoutStart_Throws()
    {
        Assert.Throws<DomainException>(() => NewSession().Complete("code", 50, "{}"));
    }

    [Fact]
    public void Complete_Twice_Throws()
    {
        var session = NewSession();
        session.Start();
        session.Complete("code", 50, "{}");

        Assert.Throws<DomainException>(() => session.Complete("otro", 90, "{}"));
        Assert.Equal(50, session.OverallScore); // no pisa la nota original
    }

    [Fact]
    public void Start_AfterCompleted_Throws()
    {
        var session = NewSession();
        session.Start();
        session.Complete("code", 50, "{}");

        Assert.Throws<DomainException>(session.Start);
    }

    [Fact]
    public void Fail_WithoutStart_Throws()
    {
        Assert.Throws<DomainException>(NewSession().Fail);
    }

    [Fact]
    public void StartPractice_WithLibraryChallenge_StartsInProgress()
    {
        var session = InterviewSession.StartPractice(Guid.NewGuid(), LibraryChallenge());

        Assert.True(session.IsPractice);
        Assert.Equal(InterviewStatus.InProgress, session.Status);
    }

    [Fact]
    public void StartPractice_WithJobOpeningChallenge_Throws()
    {
        var challenge = LibraryChallenge().CopyForJobOpening(Guid.NewGuid(), 0);

        Assert.Throws<DomainException>(() => InterviewSession.StartPractice(Guid.NewGuid(), challenge));
    }
}
