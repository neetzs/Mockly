// backend/src/Application/Features/Interviews/Validators/InterviewValidators.cs
using Mockly.Application.Features.Interviews.DTOs;
using FluentValidation;

namespace Mockly.Application.Features.Interviews.Validators;

public sealed class SubmitCodeRequestValidator : AbstractValidator<SubmitCodeRequest>
{
    public SubmitCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código no puede estar vacío.")
            .MinimumLength(10).WithMessage("El código debe tener al menos 10 caracteres.")
            .MaximumLength(50_000).WithMessage("El código no puede superar los 50.000 caracteres.");
    }
}
