using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;

namespace ArchiFlow.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ArchiFlowDbContext _context;

    public UnitOfWork(ArchiFlowDbContext context) => _context = context;

    public async Task<int> Commit(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
