using FluentValidation.TestHelper;
using StockFlow.Application.Auth.Login;

namespace StockFlow.Tests.Application.Auth.Login;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginCommand("alice", "password123"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_HasError()
    {
        var result = _validator.TestValidate(new LoginCommand(string.Empty, "password123"));

        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_HasError()
    {
        var result = _validator.TestValidate(new LoginCommand("alice", string.Empty));

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }
}
