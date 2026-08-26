namespace StockFlow.Application.Users.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : IRequest<Result>;
