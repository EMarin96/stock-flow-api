using StockFlow.Domain.Locations;

namespace StockFlow.Application.Locations.Shared;

public static class LocationErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Locations.NotFound", $"No location was found with id '{id}'.");

    public static Error CodeAlreadyExists(string code) =>
        Error.Conflict("Locations.CodeAlreadyExists", $"A location with code '{code}' already exists.");

    public static Error ValidationFailed(FluentValidation.Results.ValidationResult validationResult) =>
        Error.Validation(
            "Locations.ValidationFailed",
            string.Join(" ", validationResult.Errors.Select(failure => failure.ErrorMessage)));

    public static Error InvalidState(Country country, string state) =>
        Error.Validation(
            "Locations.InvalidState",
            $"'{state}' is not a recognized state/province for country '{country}'.");

    public static Error InvalidCity(Country country, string state, string city) =>
        Error.Validation(
            "Locations.InvalidCity",
            $"'{city}' is not a recognized city for state '{state}' in country '{country}'.");

    public static Error ReferenceDataUnavailable() =>
        Error.Unavailable(
            "Locations.ReferenceDataUnavailable",
            "The address reference-data service is currently unavailable; State/City could not be validated. Please try again later.");
}
