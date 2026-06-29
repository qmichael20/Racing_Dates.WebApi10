namespace Application.Abstractions.Data
{
    public interface IRepository<T> 
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    }
}
