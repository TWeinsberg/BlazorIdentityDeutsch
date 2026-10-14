namespace BlazorIdentityDeutsch.Services {
    /// <summary>
    /// Step3.2 generischen Datenbank Service erstellen 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DBService<TEntity> : IDBService<TEntity> where TEntity : class {
        private readonly ApplicationDbContext DbContext;
        private readonly DbSet<TEntity> DBSet;
        public DBService(ApplicationDbContext context) {
            DbContext = context;
            DBSet = DbContext.Set<TEntity>();
        }

        public void Dispose() {
            DbContext.Dispose();
            GC.SuppressFinalize(this);
        }
        public async Task<List<TEntity>> GetAllAsync() => await DBSet.ToListAsync();

        public async Task<TEntity?> GetByIdAsync(params object[] keyValues) {
            var entity = await DBSet.FindAsync(keyValues);
            if(entity is not null) { DbContext.Entry(entity).State = EntityState.Detached; }
            return entity;
        }
        public async Task<TEntity> AddAsync(TEntity entity) {
            await DBSet.AddAsync(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<(int affected, IReadOnlyCollection<TEntity> entities)> AddRangeAsync(IEnumerable<TEntity> entities) {
            // Spart eine Kopie, falls schon eine List übergeben wird.
            var list = entities as List<TEntity> ?? [.. entities];
            await DBSet.AddRangeAsync(list);
            return (await DbContext.SaveChangesAsync(), list);
        }

        public async Task<int> UpdateAsync(TEntity entity) {
            DbContext.Entry(entity).State = EntityState.Modified;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(params object[] keyValues) {
            var entity = await DBSet.FindAsync(keyValues);
            if (entity == null) return 0;

            DBSet.Remove(entity);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(params object[] keyValues) => await DBSet.FindAsync(keyValues) != null;

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate) => await DBSet.AnyAsync(predicate);

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await DBSet.Where(predicate).ToListAsync();

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
            => predicate == null
                ? await DBSet.CountAsync()
                : await DBSet.CountAsync(predicate);

        public async Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate) => await DBSet.Where(predicate).ToListAsync();

        public IQueryable<TEntity> Query() => DBSet.AsQueryable();

        public IQueryable<TResult> LookupQuery<TResult>(Expression<Func<TEntity, TResult>> selector) => DBSet.AsNoTracking().Select(selector);
    }
}
