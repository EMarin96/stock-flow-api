using StockFlow.Application.StockMovements.Shared;
using StockFlow.Domain.StockMovements;

namespace StockFlow.Application.StockMovements.CreateStockMovement;

public sealed class CreateStockMovementValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementValidator()
    {
        RuleFor(command => command.Type)
            .IsInEnum();

        RuleFor(command => command.Quantity)
            .GreaterThan(0);

        RuleFor(command => command.Direction)
            .IsInEnum()
            .When(command => command.Direction is not null);

        RuleFor(command => command)
            .Custom(ValidateLocationsAndDirection);
    }

    /// <summary>
    /// Enforces the per-Type location-field requirements and the Direction
    /// presence rule as a single cross-field check (see
    /// <see cref="Domain.StockMovements.StockMovement.Create"/>, which
    /// duplicates the same rules as defense-in-depth).
    /// </summary>
    private static void ValidateLocationsAndDirection(
        CreateStockMovementCommand command,
        ValidationContext<CreateStockMovementCommand> context)
    {
        switch (command.Type)
        {
            case MovementType.In:
                if (command.SourceLocationId is not null)
                {
                    context.AddFailure(
                        nameof(command.SourceLocationId),
                        StockMovementErrors.InvalidLocationForType("An 'In' movement cannot have a source location.").Message);
                }

                if (command.DestinationLocationId is null)
                {
                    context.AddFailure(
                        nameof(command.DestinationLocationId),
                        StockMovementErrors.InvalidLocationForType("An 'In' movement requires a destination location.").Message);
                }

                break;

            case MovementType.Out:
                if (command.DestinationLocationId is not null)
                {
                    context.AddFailure(
                        nameof(command.DestinationLocationId),
                        StockMovementErrors.InvalidLocationForType("An 'Out' movement cannot have a destination location.").Message);
                }

                if (command.SourceLocationId is null)
                {
                    context.AddFailure(
                        nameof(command.SourceLocationId),
                        StockMovementErrors.InvalidLocationForType("An 'Out' movement requires a source location.").Message);
                }

                break;

            case MovementType.Transfer:
                if (command.SourceLocationId is null)
                {
                    context.AddFailure(
                        nameof(command.SourceLocationId),
                        StockMovementErrors.InvalidLocationForType("A 'Transfer' movement requires a source location.").Message);
                }

                if (command.DestinationLocationId is null)
                {
                    context.AddFailure(
                        nameof(command.DestinationLocationId),
                        StockMovementErrors.InvalidLocationForType("A 'Transfer' movement requires a destination location.").Message);
                }

                break;

            case MovementType.Adjustment:
                if (command.SourceLocationId is not null)
                {
                    context.AddFailure(
                        nameof(command.SourceLocationId),
                        StockMovementErrors.InvalidLocationForType("An 'Adjustment' movement cannot have a source location.").Message);
                }

                if (command.DestinationLocationId is null)
                {
                    context.AddFailure(
                        nameof(command.DestinationLocationId),
                        StockMovementErrors.InvalidLocationForType(
                            "An 'Adjustment' movement requires a destination location (the location being corrected).").Message);
                }

                break;

            default:
                // Caught separately by the Type.IsInEnum() rule above.
                return;
        }

        if (command.Type == MovementType.Adjustment && command.Direction is null)
        {
            context.AddFailure(nameof(command.Direction), StockMovementErrors.DirectionRequired().Message);
        }

        if (command.Type != MovementType.Adjustment && command.Direction is not null)
        {
            context.AddFailure(nameof(command.Direction), StockMovementErrors.DirectionNotAllowed().Message);
        }
    }
}
