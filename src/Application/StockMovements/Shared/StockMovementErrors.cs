namespace StockFlow.Application.StockMovements.Shared;

/// <summary>
/// Shared errors for the StockMovements feature. "Not found" for a referenced
/// product/location deliberately reuses <c>ProductErrors.NotFound</c>/
/// <c>LocationErrors.NotFound</c> instead of duplicating them here — same
/// error code/message either way (see plan.md — Decisions).
/// </summary>
public static class StockMovementErrors
{
    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "StockMovements.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));

    /// <summary>
    /// Shared code for every "this movement type doesn't support/require this
    /// location field" validation failure; <paramref name="message"/> carries
    /// the scenario-specific detail.
    /// </summary>
    public static Error InvalidLocationForType(string message) =>
        Error.Validation("StockMovements.InvalidLocationForType", message);

    public static Error DirectionRequired() =>
        Error.Validation("StockMovements.DirectionRequired", "Direction is required for an 'Adjustment' movement.");

    public static Error DirectionNotAllowed() =>
        Error.Validation("StockMovements.DirectionNotAllowed", "Direction is only allowed for an 'Adjustment' movement.");

    public static Error CrossCountryTransfer() =>
        Error.Validation(
            "StockMovements.CrossCountryTransfer",
            "A transfer's source and destination locations must be in the same country.");

    public static Error InsufficientStock(Guid productId, Guid locationId) =>
        Error.Conflict(
            "StockMovements.InsufficientStock",
            $"Product '{productId}' does not have enough stock at location '{locationId}' to complete this movement.");
}
