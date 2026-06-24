using FluentValidation;
using LIS.Api.Models;

namespace LIS.Api.Validation;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        // Stop at the first failing rule per field so an empty value yields one message
        // ("... is required.") rather than also reporting the format rule.
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(200).WithMessage("Patient name must be 200 characters or fewer.");

        RuleFor(x => x.TestType)
            .NotEmpty().WithMessage("Test type is required.")
            .Must(value => EnumDisplay.TryParseTestType(value, out _))
            .WithMessage("Test type must be one of: CBC, BMP, Lipid Panel, UA.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(value => EnumDisplay.TryParsePriority(value, out _))
            .WithMessage("Priority must be Routine or STAT.");

        RuleFor(x => x.CollectionDate)
            .NotEmpty().WithMessage("Collection date is required.")
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Collection date cannot be in the past.");
    }
}
