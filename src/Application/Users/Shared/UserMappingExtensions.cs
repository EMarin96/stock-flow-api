using StockFlow.Domain.Users;

namespace StockFlow.Application.Users.Shared;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Username,
        user.Role,
        user.CreatedAt,
        user.CreatedBy,
        user.UpdatedAt,
        user.UpdatedBy);
}
