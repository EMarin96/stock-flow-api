namespace StockFlow.Application.Users.Shared;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Users.NotFound", $"No user was found with id '{id}'.");

    public static Error UsernameAlreadyExists(string username) =>
        Error.Conflict("Users.UsernameAlreadyExists", $"A user with username '{username}' already exists.");

    public static Error CannotDeactivateSelf() =>
        Error.Validation("Users.CannotDeactivateSelf", "An admin cannot deactivate their own account.");

    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "Users.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));
}
