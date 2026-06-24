using FluentValidation.TestHelper;
using LIS.Api.Models;
using LIS.Api.Validation;

namespace LIS.Api.Tests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Passes_WhenCredentialsProvided()
    {
        _validator.TestValidate(new LoginRequest("tech", "Tech123!")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Fails_WhenUsernameEmpty()
    {
        _validator.TestValidate(new LoginRequest("", "pw"))
            .ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Fails_WhenPasswordEmpty()
    {
        _validator.TestValidate(new LoginRequest("tech", ""))
            .ShouldHaveValidationErrorFor(x => x.Password);
    }
}
