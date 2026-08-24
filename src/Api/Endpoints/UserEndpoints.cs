using StockFlow.Api.Common;
using StockFlow.Application.Common.Mediator;
using StockFlow.Application.Users.CreateUser;
using StockFlow.Application.Users.DeactivateUser;
using StockFlow.Application.Users.GetUserById;
using StockFlow.Application.Users.GetUsers;
using StockFlow.Application.Users.Shared;
using StockFlow.Application.Users.UpdateUser;
using StockFlow.Domain.Users;

namespace StockFlow.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Every route requires the "AdminOnly" policy — user management is
        // Admin-only, per spec.md's role table.
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization("AdminOnly");

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Creates a new user.")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .WithSummary("Fetches a single user by id.")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetUsers)
            .WithName("GetUsers")
            .WithSummary("Lists users with pagination, optionally filtered by username and/or role.")
            .Produces<Application.Common.Pagination.PagedResult<UserDto>>()
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Updates a user's role and, optionally, resets their password. Username is not editable.")
            .Produces<UserDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeactivateUser)
            .WithName("DeactivateUser")
            .WithSummary("Deactivates a user (soft delete). An admin cannot deactivate their own account.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Username, request.Password, request.Role);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute("GetUserById", new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetUserById(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> GetUsers(
        IMediator mediator,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        string? username = null,
        Role? role = null)
    {
        var result = await mediator.Send(new GetUsersQuery(page, pageSize, username, role), cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        UpdateUserRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.Role, request.NewPassword);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> DeactivateUser(Guid id, IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateUserCommand(id), cancellationToken);

        return result.IsSuccess ? Results.NoContent() : result.ToProblem();
    }
}

public sealed record CreateUserRequest(string Username, string Password, Role Role);

public sealed record UpdateUserRequest(Role Role, string? NewPassword);
