using Mockly.Domain.Entities;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.UnitTests.Domain;

public class JobOpeningTests
{
    private static TechnicalChallenge LibraryChallenge() =>
        new("Reto", "Descripción", "// código", ChallengeDifficulty.Junior, ProgrammingLanguage.CSharp, jobOpeningId: null);

    private static User Candidate() => new("candidato@test.dev", "hash", "Candidato", UserRole.Candidate);

    private static JobOpening OpeningWithChallenges(int count)
    {
        var opening = new JobOpening("Backend .NET", "Búsqueda", Guid.NewGuid());
        for (var i = 0; i < count; i++)
        {
            opening.AddChallengeFromLibrary(LibraryChallenge());
        }
        return opening;
    }

    [Fact]
    public void AddChallengeFromLibrary_CopiesChallengeIntoOpening()
    {
        var opening = OpeningWithChallenges(2);

        Assert.All(opening.Challenges, c => Assert.Equal(opening.Id, c.JobOpeningId));
        Assert.Equal([0, 1], opening.Challenges.Select(c => c.Position));
    }

    [Fact]
    public void AddChallengeFromLibrary_OverMax_Throws()
    {
        var opening = OpeningWithChallenges(JobOpening.MaxChallenges);

        Assert.Throws<DomainException>(() => opening.AddChallengeFromLibrary(LibraryChallenge()));
    }

    [Fact]
    public void Invite_CreatesOneSessionPerChallenge()
    {
        var opening = OpeningWithChallenges(3);
        var candidate = Candidate();

        var sessions = opening.Invite(candidate);

        Assert.Equal(3, sessions.Count);
        Assert.All(sessions, s => Assert.Equal(candidate.Id, s.CandidateId));
        Assert.Equal(opening.Challenges.Select(c => c.Id), sessions.Select(s => s.TechnicalChallengeId));
    }

    [Fact]
    public void Invite_SameCandidateTwice_Throws()
    {
        var opening = OpeningWithChallenges(1);
        var candidate = Candidate();
        opening.Invite(candidate);

        Assert.Throws<DomainException>(() => opening.Invite(candidate));
    }

    [Fact]
    public void Invite_Recruiter_Throws()
    {
        var recruiter = new User("reclutador@test.dev", "hash", "Reclutador", UserRole.Recruiter);

        Assert.Throws<DomainException>(() => OpeningWithChallenges(1).Invite(recruiter));
    }

    [Fact]
    public void Invite_WithoutChallenges_Throws()
    {
        Assert.Throws<DomainException>(() => OpeningWithChallenges(0).Invite(Candidate()));
    }

    [Fact]
    public void Invite_ClosedOpening_Throws()
    {
        var opening = OpeningWithChallenges(1);
        opening.Deactivate();

        Assert.Throws<DomainException>(() => opening.Invite(Candidate()));
    }
}
