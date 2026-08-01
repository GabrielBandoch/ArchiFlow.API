using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;

namespace ArchiFlow.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ArchiFlowDbContext _context;
    private bool _disposed;

    public UnitOfWork(ArchiFlowDbContext context) => _context = context;

    public async Task<int> Commit(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
