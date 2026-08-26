namespace StockFlow.Application.Auth.Shared;

public static class AuthErrors
{
    /// <summary>
    /// Deliberately the same error for an unknown username, a wrong password,
    /// and a deactivated user — the caller must not be able to tell which case
    /// it was (see spec.md — acceptance criteria).
    /// </summary>
    public static Error InvalidCredentials() =>
        Error.Unauthorized("Auth.InvalidCredentials", "The username or password is incorrect.");

    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "Auth.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));
}
