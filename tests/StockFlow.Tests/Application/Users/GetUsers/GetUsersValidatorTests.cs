using FluentValidation.TestHelper;
using StockFlow.Application.Users.GetUsers;
using StockFlow.Domain.Users;

namespace StockFlow.Tests.Application.Users.GetUsers;

public class GetUsersValidatorTests
{
    private readonly GetUsersValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.TestValidate(new GetUsersQuery(1, 20, null, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenPageIsLessThan1_HasError()
    {
        var result = _validator.TestValidate(new GetUsersQuery(0, 20, null, null));

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Fact]
    public void Validate_WhenPageSizeExceedsMax_HasError()
    {
        var result = _validator.TestValidate(new GetUsersQuery(1, GetUsersValidator.MaxPageSize + 1, null, null));

        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validate_WhenRoleIsUndefined_HasError()
    {
        var result = _validator.TestValidate(new GetUsersQuery(1, 20, null, (Role)999));

        result.ShouldHaveValidationErrorFor(q => q.Role);
    }
}
