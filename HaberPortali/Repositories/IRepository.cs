using System.Linq.Expressions;

namespace HaberPortali.Repositories
{
    // Tüm tablolar için ortak veri erişim işlemleri (Repository deseni).
    // Controller'lar DbContext'e doğrudan değil, bu arayüz üzerinden erişir.
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdAsync(int id);

        Task<T?> GetFirstAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes);

        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);

        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

        // Daha karmaşık sorgular (gruplama, sayfalama vb.) için.
        IQueryable<T> Query();

        Task AddAsync(T entity);

        void Update(T entity);

        void Delete(T entity);

        Task<int> SaveAsync();
    }
}
