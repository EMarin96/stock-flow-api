using StockFlow.Application.Auth.Shared;
using StockFlow.Application.Common.Security;
using StockFlow.Application.Users.Shared;

namespace StockFlow.Application.Auth.Login;

public sealed class LoginHandler(
    IUserReadRepository userReadRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IValidator<LoginCommand> validator) : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<LoginResponseDto>(AuthErrors.ValidationFailed(validationResult));
        }

        var authenticationRecord = await userReadRepository.GetForAuthenticationAsync(request.Username, cancellationToken);

        // Unknown username, deactivated user, or wrong password all return the
        // exact same error — no distinguishable signal for enumeration (see
        // spec.md — acceptance criteria).
        if (authenticationRecord is null
            || authenticationRecord.IsDeleted
            || !passwordHasher.Verify(request.Password, authenticationRecord.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(AuthErrors.InvalidCredentials());
        }

        var (token, expiresAt) = jwtTokenGenerator.GenerateToken(
            authenticationRecord.Id,
            authenticationRecord.Username,
            authenticationRecord.Role);

        return Result.Success(new LoginResponseDto(token, expiresAt));
    }
}
