namespace StockFlow.Application.Common.Persistence;

/// <summary>
/// Centralizes committing write-side changes across one or more repositories in a
/// single transaction, and translates database errors (e.g. unique-constraint
/// violations) into Application-level exceptions, so that translation doesn't need
/// to be duplicated in every write repository.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
