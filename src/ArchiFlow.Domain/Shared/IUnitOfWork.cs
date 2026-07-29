namespace ArchiFlow.Domain.Shared;

public interface IUnitOfWork : IDisposable
{
    Task<int> Commit(CancellationToken cancellationToken = default);
}
