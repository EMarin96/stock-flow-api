using StockFlow.Api.Common;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.Countries.GetCities;
using StockFlow.Application.Countries.GetCountries;
using StockFlow.Application.Countries.GetStates;
using StockFlow.Application.Countries.Shared;

namespace StockFlow.Api.Endpoints;

public static class CountryEndpoints
{
    public static IEndpointRouteBuilder MapCountryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/countries").WithTags("Countries");

        group.MapGet("/", GetCountries)
            .WithName("GetCountries")
            .WithSummary("Lists the countries StockFlow currently supports.")
            .Produces<IReadOnlyList<CountryDto>>();

        group.MapGet("/{code}/states", GetStates)
            .WithName("GetStates")
            .WithSummary("Lists the recognized states/provinces for a supported country.")
            .Produces<IReadOnlyList<StateDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapGet("/{code}/states/{state}/cities", GetCities)
            .WithName("GetCities")
            .WithSummary("Lists the recognized cities for a given country/state combination.")
            .Produces<IReadOnlyList<CityDto>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> GetCountries(IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCountriesQuery(), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetStates(string code, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStatesQuery(code), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetCities(
        string code,
        string state,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCitiesQuery(code, state), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
