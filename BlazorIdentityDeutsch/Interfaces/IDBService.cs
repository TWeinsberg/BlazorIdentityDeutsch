namespace BlazorIdentityDeutsch.Interfaces {
    /// <summary>
    /// Step3.1 generisches Datenbank Interface erstellen 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IDBService<TEntity> : IDisposable where TEntity : class {
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(params object[] keyValues);
        Task<TEntity> AddAsync(TEntity entity);
        Task<(int affected, IReadOnlyCollection<TEntity> entities)> AddRangeAsync(IEnumerable<TEntity> entities);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> DeleteAsync(params object[] keyValues);
        Task<bool> ExistsAsync(params object[] keyValues);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);

        Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);
        IQueryable<TEntity> Query();

        IQueryable<TResult> LookupQuery<TResult>(Expression<Func<TEntity, TResult>> selector);
    }
}
