using Mockly.Application.Features.Auth.DTOs;
using Mockly.Application.Features.Auth.Validators;
using Mockly.Application.Features.Interviews.DTOs;
using Mockly.Application.Features.Interviews.Validators;
using Mockly.Application.Features.Recruiter.DTOs;
using Mockly.Application.Features.Recruiter.Validators;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;

namespace Mockly.UnitTests.Application;

public class ValidatorTests
{
    [Fact]
    public void Register_ValidRequest_Passes()
    {
        var result = new RegisterRequestValidator().Validate(new RegisterRequest("ana@test.dev", "Segura123", "Ana", UserRole.Candidate));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("corta1A")]      // menos de 8 caracteres
    [InlineData("sinmayuscula1")]
    [InlineData("SinNumeros")]
    public void Register_WeakPassword_Fails(string password)
    {
        var result = new RegisterRequestValidator().Validate(new RegisterRequest("ana@test.dev", password, "Ana", UserRole.Candidate));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void Register_InvalidRole_Fails()
    {
        var result = new RegisterRequestValidator().Validate(new RegisterRequest("ana@test.dev", "Segura123", "Ana", (UserRole)99));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Role));
    }

    [Fact]
    public void Login_InvalidEmail_Fails()
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest("no-es-email", "x"));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("corto")]
    public void SubmitCode_TooShort_Fails(string code)
    {
        Assert.False(new SubmitCodeRequestValidator().Validate(new SubmitCodeRequest(code)).IsValid);
    }

    [Fact]
    public void SubmitCode_TooLong_Fails()
    {
        Assert.False(new SubmitCodeRequestValidator().Validate(new SubmitCodeRequest(new string('a', 50_001))).IsValid);
    }

    [Fact]
    public void CreateJobOpening_ValidRequest_Passes()
    {
        var request = new CreateJobOpeningRequest("Backend .NET", "", [Guid.NewGuid(), Guid.NewGuid()]);

        Assert.True(new CreateJobOpeningRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void CreateJobOpening_NullChallengeIds_FailsWithoutThrowing()
    {
        var request = new CreateJobOpeningRequest("Backend .NET", "", null!);

        Assert.False(new CreateJobOpeningRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void CreateJobOpening_TooManyChallenges_Fails()
    {
        var ids = Enumerable.Range(0, JobOpening.MaxChallenges + 1).Select(_ => Guid.NewGuid()).ToList();

        Assert.False(new CreateJobOpeningRequestValidator().Validate(new CreateJobOpeningRequest("Backend", "", ids)).IsValid);
    }

    [Fact]
    public void CreateJobOpening_DuplicateChallenges_Fails()
    {
        var id = Guid.NewGuid();

        Assert.False(new CreateJobOpeningRequestValidator().Validate(new CreateJobOpeningRequest("Backend", "", [id, id])).IsValid);
    }
}
