using ArchiFlow.Domain.Shared;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ArchiFlowDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ArchiFlowDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAll() =>
        await _dbSet.AsNoTracking().ToListAsync();

    public async Task<T?> GetById(Guid id) =>
        await _dbSet.FindAsync(id);

    public async Task<T> Create(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public Task<T> Update(T entity)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public async Task<T> CreateOrUpdate(T entity, Guid id)
    {
        var exists = await Exists(id);
        return exists ? await Update(entity) : await Create(entity);
    }

    public async Task Delete(Guid id)
    {
        var entity = await GetById(id)
            ?? throw new KeyNotFoundException($"Entidade com ID {id} não encontrada.");

        _dbSet.Remove(entity);
    }

    public async Task<bool> Exists(Guid id) =>
        await _dbSet.FindAsync(id) is not null;
}
