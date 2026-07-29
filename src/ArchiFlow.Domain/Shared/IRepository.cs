namespace ArchiFlow.Domain.Shared;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll();
    Task<T?> GetById(Guid id);
    Task<T> Create(T entity);
    Task<T> Update(T entity);
    Task<T> CreateOrUpdate(T entity, Guid id);
    Task Delete(Guid id);
    Task<bool> Exists(Guid id);
}
