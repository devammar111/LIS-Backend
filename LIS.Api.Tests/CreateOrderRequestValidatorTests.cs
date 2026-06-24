using FluentValidation.TestHelper;
using LIS.Api.Models;
using LIS.Api.Validation;

namespace LIS.Api.Tests;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    private static CreateOrderRequest Valid() => new()
    {
        PatientName = "John Smith",
        TestType = "CBC",
        Priority = "Routine",
        CollectionDate = DateOnly.FromDateTime(DateTime.Today)
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_WhenPatientNameEmpty()
    {
        var request = Valid();
        request.PatientName = "";
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.PatientName)
            .WithErrorMessage("Patient name is required.");
    }

    [Fact]
    public void Fails_WhenTestTypeInvalid()
    {
        var request = Valid();
        request.TestType = "XYZ";
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.TestType)
            .WithErrorMessage("Test type must be one of: CBC, BMP, Lipid Panel, UA.");
    }

    [Fact]
    public void Fails_WhenPriorityInvalid()
    {
        var request = Valid();
        request.Priority = "Urgent";
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("Priority must be Routine or STAT.");
    }

    [Fact]
    public void Fails_WhenCollectionDateInPast()
    {
        var request = Valid();
        request.CollectionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        _validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.CollectionDate)
            .WithErrorMessage("Collection date cannot be in the past.");
    }

    [Fact]
    public void Accepts_LipidPanelWithSpace()
    {
        var request = Valid();
        request.TestType = "Lipid Panel";
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.TestType);
    }
}
