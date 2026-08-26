using FluentValidation.TestHelper;
using StockFlow.Application.Users.CreateUser;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.CreateUser;

public class CreateUserValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    private static CreateUserCommand ValidCommand() => new("alice", "password123", Role.Operator);

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_HasError()
    {
        var command = ValidCommand() with { Username = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Fact]
    public void Validate_WhenPasswordIsShorterThan8Characters_HasError()
    {
        var command = ValidCommand() with { Password = "short1" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Password);
    }

    [Fact]
    public void Validate_WhenRoleIsUndefined_HasError()
    {
        var command = ValidCommand() with { Role = (Role)999 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Role);
    }
}
