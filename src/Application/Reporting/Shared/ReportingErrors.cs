namespace StockFlow.Application.Reporting.Shared;

public static class ReportingErrors
{
    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "Reporting.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));
}
