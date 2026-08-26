namespace StockFlow.Application.Auth.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<LoginResponseDto>>;

public sealed record LoginResponseDto(string Token, DateTime ExpiresAt);
