// backend/src/Application/Features/Recruiter/Validators/RecruiterValidators.cs
using FluentValidation;
using Mockly.Application.Features.Recruiter.DTOs;
using Mockly.Domain.Entities;

namespace Mockly.Application.Features.Recruiter.Validators;

public sealed class CreateJobOpeningRequestValidator : AbstractValidator<CreateJobOpeningRequest>
{
    public CreateJobOpeningRequestValidator()
    {
        // Los máximos coinciden con las columnas de la base (JobOpeningConfiguration).
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(200).WithMessage("El título no puede superar los 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("La descripción no puede superar los 4000 caracteres.");

        // Stop: si la lista falta (null), las reglas siguientes no se evalúan (si no, revientan con NullReference).
        RuleFor(x => x.ChallengeIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Elegí al menos un reto.")
            .Must(ids => ids.Count <= JobOpening.MaxChallenges)
                .WithMessage($"Podés elegir como máximo {JobOpening.MaxChallenges} retos.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Hay retos repetidos.");
    }
}

public sealed class InviteCandidateRequestValidator : AbstractValidator<InviteCandidateRequest>
{
    public InviteCandidateRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.");
    }
}
