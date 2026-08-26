using FluentValidation.TestHelper;
using StockFlow.Application.Users.UpdateUser;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.UpdateUser;

public class UpdateUserValidatorTests
{
    private readonly UpdateUserValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(new UpdateUserCommand(Guid.NewGuid(), Role.Admin, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenIdIsEmpty_HasError()
    {
        var result = _validator.TestValidate(new UpdateUserCommand(Guid.Empty, Role.Admin, null));

        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Fact]
    public void Validate_WhenRoleIsUndefined_HasError()
    {
        var result = _validator.TestValidate(new UpdateUserCommand(Guid.NewGuid(), (Role)999, null));

        result.ShouldHaveValidationErrorFor(c => c.Role);
    }

    [Fact]
    public void Validate_WhenNewPasswordIsShorterThan8Characters_HasError()
    {
        var result = _validator.TestValidate(new UpdateUserCommand(Guid.NewGuid(), Role.Admin, "short1"));

        result.ShouldHaveValidationErrorFor(c => c.NewPassword);
    }

    [Fact]
    public void Validate_WhenNewPasswordIsNull_HasNoError()
    {
        var result = _validator.TestValidate(new UpdateUserCommand(Guid.NewGuid(), Role.Admin, null));

        result.ShouldNotHaveValidationErrorFor(c => c.NewPassword);
    }
}
