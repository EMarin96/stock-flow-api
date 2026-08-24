using StockFlow.Api.Common;
using StockFlow.Application.Auth.Login;
using StockFlow.Application.Common.Mediator;

namespace StockFlow.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticates with a username and password, returning a JWT and its expiry.")
            .Produces<LoginResponseDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Username, request.Password);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}

public sealed record LoginRequest(string Username, string Password);
