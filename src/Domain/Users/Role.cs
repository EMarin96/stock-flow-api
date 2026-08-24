namespace StockFlow.Domain.Users;

/// <summary>
/// The fixed set of user roles (see constitution/tech-stack.md — Data/domain
/// model). Every user has exactly one role, which determines their
/// permissions across every other feature.
/// </summary>
public enum Role
{
    Admin,
    Operator,
    ReadOnly,
}
